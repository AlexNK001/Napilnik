﻿using System;
//1) По вашему вопросу. Необходим объект заказа - Order.
//У корзины должен быть метод (не забываем, что методы глаголами именуем),
//который умеет формировать заказ на основании того, что лежит в корзине.
//И у заказа есть какая-то ссылка для оплаты.

//2) Goods - не корректный термин. Goods это товары, но Good - это хорошо. Нужно другое слово.

//3) Не забываем про предыдущие уроки. Везде должны быть конструкторы
//и валидация входных параметров с необходимыми исключениями.

//4) Корзина должна иметь ссылку на склад, чтоб проверять там наличие товара,
//но не полноценную, а по урезанному интерфейсу,
//который позволит только проверять наличие, бронировать и удалять товар.

//5) В момент покупки (НЕ добавления в корзину) товары со склада должны удаляться

//6) В момент добавления в корзину и проверки склада учитывайте, сколько их уже есть в корзине.

//7) GetProduct - тоже должна быть проверка на не отрицательность входящего значения.

//8) GetProduct - if (_count - productCount < 0) это не ArgumentOutOfRangeException, а скорее не валидная операция. 

//9) public int Count => _count; -что мешает сделать автосвойство сразу?

//10) public Cart Cart() -метод с глагола именуем

//11) метод Get у склада -  от методов с именем Get,
//по аналогии со свойствами, ожидается, что они что-то возвращают (мы получает наружу). Нужно другое слово.

namespace online_store
{
    public class Shop
    {
        private readonly Warehouse _warehouse;

        public Shop(Warehouse warehouse)
        {
            _warehouse = warehouse ?? throw new ArgumentNullException(nameof(warehouse));
        }

        public Cart Cart()
        {
            if (_warehouse == null)
                throw new ArgumentNullException(nameof(_warehouse));

            return new Cart(_warehouse);
        }
    }
}
