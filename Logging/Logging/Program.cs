using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

public static class Program
{
    private static void Main()
    {
        ViewFabric viewFabric = new ViewFabric();
        DatabaseContext databaseContext = new DatabaseContext();
        HashCreator hashCreator = new HashCreator();
        Repository repository = new Repository(databaseContext, hashCreator);
        Presenter presenter = new Presenter(viewFabric, repository);
    }
}

public class Presenter
{
    private readonly View _view;
    private readonly Repository _repository;

    public Presenter(ViewFabric viewFabric, Repository repository)
    {
        if (viewFabric == null)
            throw new ArgumentNullException(nameof(viewFabric));
        else
            _view = viewFabric.CreateView(this);

        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public void HandleRawData(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentNullException(nameof(text));

        try
        {
            Passport passport = new Passport(text);
            Citizen citizen = _repository.FindCitezen(passport);

            string message;

            if (citizen == null)
                message = MessageStorage.EnteringPassportData;
            else
                message = citizen.IsVoted ? MessageStorage.AccessGranted : MessageStorage.AccessNotGranted;

            _view.ShowPassport(message, passport.Data);
        }
        catch (Exception exception)
        {
            _view.ShowException(exception.Message);
        }
    }
}

public class Repository
{
    private readonly DatabaseContext _databaseContext;
    private readonly HashCreator _hashCreator;

    public Repository(DatabaseContext databaseContext, HashCreator hashCreator)
    {
        _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        _hashCreator = hashCreator ?? throw new ArgumentNullException(nameof(hashCreator));
    }

    public Citizen FindCitezen(Passport passport)
    {
        string hash = _hashCreator.ComputeSha256Hash(passport.Data);
        DataTable dataTable = _databaseContext.CreateDataTable(hash);

        if (dataTable.Rows.Count > 0)
        {
            bool isVoted = Convert.ToBoolean(dataTable.Rows[0].ItemArray[1]);
            return new Citizen(isVoted);
        }
        else
        {
            return null;
        }
    }
}

public class DatabaseContext
{
    public DataTable CreateDataTable(string hash)
    {
        try
        {
            string commandText = string.Format("select * from passports where num='{0}' limit 1;", hash);
            string location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string connectionString = string.Format("Data Source={0}\\db.sqlite", location);

            SQLiteConnection connection = new SQLiteConnection(connectionString);
            connection.Open();
            SQLiteCommand sqLiteCommand = new SQLiteCommand(commandText, connection);
            SQLiteDataAdapter sqLiteDataAdapter = new SQLiteDataAdapter(sqLiteCommand);
            DataTable dataTable = new DataTable();
            sqLiteDataAdapter.Fill(dataTable);
            connection.Close();
            return dataTable;
        }
        catch (SQLiteException exception)
        {
            if (exception.ErrorCode != 1)
                throw new FileNotFoundException("Файл db.sqlite не найден. Положите файл в папку вместе с exe.");

            return new DataTable();
        }
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

public class Citizen
{
    public Citizen(bool isVoted)
    {
        IsVoted = isVoted;
    }

    public bool IsVoted { get; }
}

public class Passport
{
    private const int PassportDataLength = 10;

    public Passport(string rawData)
    {
        if (string.IsNullOrWhiteSpace(rawData))
            throw new ArgumentNullException(nameof(rawData));

        string data = rawData.Trim().Replace(" ", string.Empty);

        if (data.Length == PassportDataLength)
            Data = data;
        else
            throw new InvalidOperationException(MessageStorage.InvalidInput);
    }

    public string Data { get; }
}

public class View
{
    private readonly TextBox _passport;
    private readonly TextBox _message;
    private readonly Presenter _presenter;

    public View(TextBox passport, TextBox message, Presenter presenter)
    {
        _passport = passport ?? throw new ArgumentNullException(nameof(passport));
        _message = message ?? throw new ArgumentNullException(nameof(message));
        _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
    }

    private void OnClick(object sender, EventArgs e)
    {
        _presenter.HandleRawData(_passport.Text);
    }

    public void ShowPassport(string status, string passportData)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentNullException(nameof(status));

        if (string.IsNullOrWhiteSpace(passportData))
            throw new ArgumentNullException(nameof(passportData));

        _passport.Text = string.Format(status, passportData);
    }

    public void ShowException(string errorMessage)
    {
        _message.Text = errorMessage;
    }
}

public class ViewFabric
{
    public View CreateView(Presenter presenter)
    {
        return new View(new TextBox(), new TextBox(), presenter);
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

public class TextBox
{
    public string Text { get; set; }
}
