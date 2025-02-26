using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

public class Passport
{
    private const int PassportDataLength = 10;

    public Passport(string rawData)
    {
        if (rawData == null)
            throw new ArgumentNullException(nameof(rawData));

        if (CanData(rawData))
            Data = rawData;
        else
            throw new InvalidOperationException(MessageStorage.InvalidInput);
    }

    public string Data { get; }

    public static bool CanData(string data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        return data.Trim().Replace(" ", string.Empty).Length == PassportDataLength;
    }
}

public static class MessageStorage
{
    public const string AccessGranted = "По паспорту «{0}» доступ к бюллетеню на дистанционном электронном голосовании ПРЕДОСТАВЛЕН";
    public const string AccessNotGranted = "По паспорту «{0}» доступ к бюллетеню на дистанционном электронном голосовании НЕ ПРЕДОСТАВЛЯЛСЯ";
    public const string PassportNotFound = "Паспорт «{0}» в списке участников дистанционного голосования НЕ НАЙДЕН";

    public const string EnteringPassportData = "Введите серию и номер паспорта";
    public const string InvalidInput = "Неверный формат серии или номера паспорта";

    public const string FileNotFound = "Файл db.sqlite не найден. Положите файл в папку вместе с exe.";
}

public class View
{
    private readonly Dictionary<MessageToPassport, string> _messageToPassport;
    private readonly Dictionary<MessageToResult, string> _messageToResult;

    private readonly TextBox _passport;
    private readonly TextBox _result;
    private readonly TextBox _message;
    private readonly Presenter _presenter;

    public View(TextBox passport, TextBox result, TextBox message, Presenter presenter)
    {
        _passport = passport ?? throw new ArgumentNullException(nameof(passport));
        _result = result ?? throw new ArgumentNullException(nameof(result));
        _message = message ?? throw new ArgumentNullException(nameof(message));
        _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));

        _messageToPassport = new Dictionary<MessageToPassport, string>
        {
            { MessageToPassport.AccessGranted, MessageStorage.AccessGranted },
            { MessageToPassport.AccessNotGranted, MessageStorage.AccessNotGranted },
            { MessageToPassport.PassportNotFound, MessageStorage.PassportNotFound }
        };

        _messageToResult = new Dictionary<MessageToResult, string>
        {
            { MessageToResult.EnteringPassportData, MessageStorage.EnteringPassportData },
            { MessageToResult.InvalidInput, MessageStorage.InvalidInput }
        };
    }

    private void OnClick(object sender, EventArgs e)
    {
        _presenter.SetRawData(_passport.Text);
    }

    public void ShowPassport(MessageToPassport status, string text)
    {
        if (_messageToPassport.ContainsKey(status) == false)
            throw new ArgumentOutOfRangeException(nameof(status));

        if (text == null)
            throw new ArgumentNullException(nameof(text));

        _passport.Text = string.Format(_messageToPassport[status], text);
    }

    public void ShowResult(MessageToResult result)
    {
        if (_messageToResult.ContainsKey(result) == false)
            throw new ArgumentOutOfRangeException(nameof(result));

        _result.Text = _messageToResult[result];
    }

    public void ShowFileNotFound()
    {
        _message.Text = MessageStorage.FileNotFound;
    }
}

public class Model
{
    private const int ErrorCode = 1;

    private HashCreator _hashCreator;

    public Model(HashCreator hashCreator)
    {
        _hashCreator = hashCreator ?? throw new ArgumentNullException(nameof(hashCreator));
    }

    public bool TryFindPassportToBD(Passport passport, out MessageToPassport message)
    {
        string location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        try
        {
            var connection = CreateConnection(location, passport.Data);
            var sqLiteDataAdapter = CreateAdapter(connection);
            var dataTable = new DataTable();

            connection.Open();
            sqLiteDataAdapter.Fill(dataTable);
            connection.Close();

            message = GetMessage(dataTable);

            return true;
        }
        catch
        {
            message = MessageToPassport.none;
            return false;
        }
    }

    public bool TryCreatePassport(string text, out Passport passport, out MessageToResult passportMessage)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));

        if (Passport.CanData(text))
        {
            passport = new Passport(text);
            passportMessage = MessageToResult.none;
            return true;
        }
        else
        {
            passport = null;
            passportMessage = text == string.Empty ? MessageToResult.InvalidInput : MessageToResult.EnteringPassportData;
            return false;
        }
    }

    private static MessageToPassport GetMessage(DataTable dataTable)
    {
        MessageToPassport message;

        if (dataTable.Rows.Count > 0)
        {
            bool isConvert = Convert.ToBoolean(dataTable.Rows[0].ItemArray[1]);
            message = isConvert ? MessageToPassport.AccessGranted : MessageToPassport.AccessNotGranted;
        }
        else
        {
            message = MessageToPassport.PassportNotFound;
        }

        return message;
    }

    private dynamic CreateConnection(string location, string rawData)
    {
        string connectionString = $"Data Source={location}\\db.sqlite";
        string hash = _hashCreator.ComputeSha256Hash(rawData);
        string commandText = $"select * from passports where num='{hash}' limit 1;";

        return new SQLiteConnection(connectionString);
    }

    private dynamic CreateAdapter(dynamic connection)
    {
        var liteCommand = new SQLiteCommand(commandText, connection);

        return new SQLiteDataAdapter(liteCommand);
    }
}

public class Presenter
{
    private readonly View _view;
    private readonly Model _model;

    public Presenter(View view, Model model)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    public void SetRawData(string text)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));

        if (_model.TryCreatePassport(text, out Passport passport, out MessageToResult passportMessage))
            FindPassport(passport);
        else
            _view.ShowResult(passportMessage);
    }

    private void FindPassport(Passport passport)
    {
        if (_model.TryFindPassportToBD(passport, out MessageToPassport message))
            _view.ShowPassport(message, passport.Data);
        else
            _view.ShowFileNotFound();
    }
}

public class HashCreator
{
    public string ComputeSha256Hash(string rawData)
    {
        var sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(rawData);
        byte[] hash = sha256.ComputeHash(bytes);
        return Encoding.UTF8.GetString(hash);
    }
}

public enum MessageToPassport
{
    none,
    AccessGranted,
    AccessNotGranted,
    PassportNotFound
}

public enum MessageToResult
{
    none,
    EnteringPassportData,
    InvalidInput
}

public class TextBox
{
    public string Text { get; set; }
}
