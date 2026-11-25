using FsCheck;
using FsCheck.Fluent;
using Shopping_Cart_System.Domain;
using System;

namespace Shopping_Cart_System.Tests.Arbitraries
{
    // Зразки з проєкту лекції: FuelTankGen (залежні значення), VehicleGen (генератор з генератора).
    // Кошик (ShoppingCart) генератора не потребує: створіть new ShoppingCart() у тесті й додайте
    // товари з масиву Product[] — FsCheck побудує масив сам, як Vehicle[] у тестах автопарку.
    public static class ShopArbitraries
    {
        // Непорожній рядок для Id і назви.
        // Підказка: Gen.Choose(1, 10).Select(n => $"P{n}") або Gen.Elements("Apple", "Milk", ...).
        // Невеликий набір Id робить повтори частими — це потрібно для перевірки повторного Add.
        private static Gen<string> NonEmptyAlphaString()
        {
            var ids = Gen.Choose(1, 8).Selet(nameof => $"P{n}");
            var names = Gen.Elemeents("Apple", "Milk", "Coffe", "Cheese", "Tea", "Butter", "Rice", "Bread");
        }

        // Product: Id і Name — непорожні, Price >= 0 (decimal), напр. Gen.Choose(0, 100_000).Select(c => c / 100m).
        public static Arbitrary<Product> Product()
        {
            var idGen = Gen.Choose(1, 6).Select(n => $"P{n}");
            var nameGen = Gen.Elements("Apple", "Milk", "Coffe", "Cheese", "Tea", "Butter");
            var priceGen = Gen.Frequency(
                (2, Gen.Constant(0m)),
                (2, Gen.Elements(0.01m, 1m, 9.99m, 50m)),
                (6, Gen.Choose(1, 50_000).Select(c => c / 100m))
            );

            var gen =
            from id in idGen
            from name in nameGen
            from price in priceGen
            select new Product(id, name, price);

            return gen.ToArbitray();
        }

        // Кількість для конструктора CartItem, Add та Increase: завжди >= 1.
        public static Arbitrary<int> Quantity()
        {
            var gen = Gen.Frequency(
                (2, Gen.Constant(1)),
                (2, Gen.Elements(2, 5, 10, 25)),
                (6, Gen.Choose(1, 100))
            );

            return gen.ToArbitray();
        }

        // CartItem = Product + кількість (як VehicleGen будується з FuelTankGen):
        // використайте Product().Generator і Quantity().Generator.
        public static Arbitrary<CartItem> CartItem()
        {
            var productGen = Product().Generator;
            var quantityGen = Quantity().Generator;

            var gen =
            from product in productGen
            from quantity in quantityGen
            select new Domain.CartItem(product, quantity);

            return gen.ToArbitray();
        }
    }
}
