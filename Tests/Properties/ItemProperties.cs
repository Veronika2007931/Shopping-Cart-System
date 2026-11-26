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


    public class ItemProperties
    {
        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public Property Increase_then_decrease_restores_quantity_when(CartItem item, int amount)
        {
            bool applicable = amount > 0;
            bool ok = true;

            if (applicable)
            {
                var copy = new CartItem(item.Product, item.Quantity);
                int initialQty = copy.Quantity;

                copy.Increase(amount);
                copy.Decrease(amount);

                ok = copy.Quantity == initialQty;
            }

            return ok.When(applicable);
        }

        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public Property Decrease_keeps_quantity_positive_when(CartItem item, int amount)
        {
            bool applicable = amount > 0 && item.Quantity - amount > 0;
            bool ok = true;

            if (applicable)
            {
                var copy = new CartItem(item.Product, item.Quantity);
                copy.Decrease(amount);
                ok = copy.Quantity > 0;

            }

            return ok.When(applicable);
        }

        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public void Constructor_with_non_positive_quantity_throws_ArgumentException(Product product, int rawQty)
        {
            int nonPositiveQty = rawQty > 0 ? -rawQty : rawQty;

            Action act = () => _ = new CartItem(product, nonPositiveQty);

            act.Should().Throw<ArgumentException>();
        }

        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public void Decrease_below_one_throws_InvalidOperationException(CartItem item, int extra)
        {
            var copy = new CartItem(item.Product, item.Quantity);
            int excessiiveAmount = item.Quantity + Math.Abs(extra);

            Action act = () => copy.Decrease(excessiiveAmount);

            act.Should().Throw<InvalidOperationException>();

        }
    }

}
