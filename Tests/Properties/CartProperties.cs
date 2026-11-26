using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FsCheck.Xunit;
using Shopping_Cart_System.Domain;
using Shopping_Cart_System.Tests.Arbitraries;
using Xunit;
using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;


namespace Shopping_Cart_System.Tests.Properties
{
    public class CartProperties
    {
        private static ShoppingCart BuildCart(Product[] products, int qty)
        {
            var cart = new ShoppingCart();
            foreach (var p in products)
            {
                cart.Add(p, qty);
            }
            return cart;
        }

        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public void TotalPrice_is_always_non_negative_and_equals_items_sum(Product[] products, int qty)
        {
            var cart = BuildCart(products, qty);
            decimal expected = cart.Items.Sum(i => i.Product.Price * i.Quantity);

            cart.TotalPrice().Should().BeGreaterThanOrEqualTo(0m);
            cart.TotalPrice().Should().Be(expected);
        }

        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public Property Add_then_remove_restore_cart_state_when(Product[] initialProducts, Product candidate, int qty)
        {
            var cart = BuildCart(initialProducts, qty);
            var freshCandidate = candidate with { Id = $"TEMP_{candidate.Id}" };
            bool applicable = qty > 0;
            bool ok = true;

            if (applicable)
            {
                decimal beforeTotal = cart.TotalPrice();
                int beforeCount = cart.Items.Count;

                cart.Add(freshCandidate, qty);
                cart.Remove(freshCandidate.Id);

                ok = cart.TotalPrice() == beforeTotal && cart.Items.Count == beforeCount;
            }

            return ok.When(applicable);
        }

        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public void Adding_same_product_twice_equals_single_add_of_double_quantity(Product product, int qty)
        {
            var uniqueProduct = product with { Id = "P-PATH" };

            var cartTwoAdds = new ShoppingCart();
            cartTwoAdds.Add(uniqueProduct, qty);
            cartTwoAdds.Add(uniqueProduct, qty);

            var cartOneAdd = new ShoppingCart();
            cartOneAdd.Add(uniqueProduct, qty * 2);

            var itemTwo = cartTwoAdds.Items.Single(i => i.Product.Id == uniqueProduct.Id);
            var itemOne = cartOneAdd.Items.Single(i => i.Product.Id == uniqueProduct.Id);

            itemTwo.Quantity.Should().Be(itemOne.Quantity);
            cartTwoAdds.TotalPrice().Should().Be(cartOneAdd.TotalPrice());

        }

        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public Property Adding_priced_product_increases_total_when(Product[] initialProducts, int baseQty, Product extra, int extraQty)
        {
            var cart = BuildCart(initialProducts, baseQty);
            var distinctExtra = extra with { Id = $"NEW_{extra.Id}" };
            bool applicable = extra.Price > 0m && extraQty > 0;
            bool ok = true;

            if (applicable)
            {
                decimal beforeTotal = cart.TotalPrice();
                cart.Add(distinctExtra, extraQty);
                ok = cart.TotalPrice() > beforeTotal;
            }

            return ok.When(applicable);
        }

        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public void Remove_missing_product_throws_InvalidOperationException(Product[] products, int qty, string missingId)
        {
            var cart = BuildCart(products, qty);
            string absentId = cart.Items.Any(i => i.Product.Id == missingId)
            ? $"ABSENT_{missingId}_{Guid.NewGuid()}"
            : missingId;

            Action act = () => cart.Remove(absentId);
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
