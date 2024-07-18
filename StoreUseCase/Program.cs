using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StoreUseCase
{
    internal class Program
    {
        static void Main(string[] args)
        {            
            Good iPhone12 = new Good("IPhone 12");
            Good iPhone11 = new Good("IPhone 11");

            Warehouse warehouse = new Warehouse();

            Shop shop = new Shop(warehouse);

            warehouse.Delive(iPhone12, 10);
            warehouse.Delive(iPhone11, 1);

            //Вывод всех товаров на складе с их остатком
            warehouse.ShowGoodsInformation();

            Cart cart = shop.Cart();
            cart.Add(iPhone12, 4);
            cart.Add(iPhone11, 3); //при такой ситуации возникает ошибка так, как нет нужного количества товара на складе

            //Вывод всех товаров в корзине
            cart.ShowGoodsInformation();

            Console.WriteLine(cart.Order().Paylink);

            cart.Add(iPhone12, 9); //Ошибка, после заказа со склада убираются заказанные товары

            Console.ReadKey();
        }
    }   

    class Shop
    {
        private Warehouse _warehouse;

        public Shop(Warehouse warehouse)
        {
            _warehouse = warehouse;
        }

        public Cart Cart()
        {
            return new Cart(this);
        }

        public bool HasEnoughGood(Good good, int quantity)
        {
            return _warehouse.IsAvailable(good, quantity);
        }

        public bool TryTakeGood(Good good, int quantity)
        {
            return _warehouse.IsAvailable(good, quantity, true);
        }
    }

    class Good
    {
        public Good(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }

    class GoodsList
    {
        private List<Cell> _cells;

        public GoodsList()
        {
            _cells = new List<Cell> ();
        }

        public IReadOnlyList<Cell> Cells => _cells;

        public void AddGood(Good good, int quantity)
        {
            Cell newCell = new Cell(good, quantity);

            int cellIndex = GetGoodIndex(good);

            if (cellIndex != -1)
                _cells[cellIndex].Merge(newCell);
            else
                _cells.Add(newCell);
        }

        public void ShowGoodsInformation()
        {
            foreach (Cell cell in _cells)            
                Console.WriteLine($"{cell.Good.Name}: {cell.Quantity} шт.");            
        }

        protected int GetGoodIndex(Good good)
        {
            return _cells.FindIndex(cell => cell.Good == good);
        }
    }

    class Cart : GoodsList
    {
        private Shop _shop;

        public Cart(Shop shop)
        {
            _shop = shop;
        }

        public void Add(Good good, int quantity)
        {
            if (_shop.HasEnoughGood(good, quantity))            
                AddGood(good, quantity);
        }

        public Order Order()
        {
            foreach (Cell cell in Cells)
                _shop.TryTakeGood(cell.Good, cell.Quantity);

            return new Order("paylink");
        }
    }    

    class Warehouse : GoodsList
    { 
        public void Delive(Good good, int quantity)
        {
           AddGood(good, quantity);
        }

        public bool IsAvailable(Good good, int quantity, bool take = false)
        {
            int cellIndex = GetGoodIndex(good);

            if (cellIndex == -1)
                throw new Exception("Product was not found in store"); 

            if (take)            
                Cells[cellIndex].Decrease(quantity);            
            else
                Cells[cellIndex].CheckQuantity(quantity);

            return true;
        }
    }

    class Cell
    {   
        public Cell(Good good, int quantity)
        {
            Utils.CheckIntRangeVariable(quantity, nameof(quantity));

            Good = good;
            Quantity = quantity;
        }

        public Good Good { get; private set; }
        public int Quantity { get; private set; }

        public void Merge(Cell cell)
        {
            if (Good != cell.Good)
                throw new Exception("Type of good does not match");

            Quantity += cell.Quantity;
        }

        public void CheckQuantity(int quantity)
        {
            if (Quantity < quantity)
                throw new Exception("Invalid quantity");
        }

        public void Decrease(int quantity)
        {
            CheckQuantity(quantity);
            Quantity -= quantity;
        }
    }

    class Order
    {
        public Order(string paylink)
        {
            Paylink = paylink;
        }

        public string Paylink { get; private set; }
    }

    static class Utils
    {
        static public void CheckIntRangeVariable(int value, string name)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
