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


    public class RepositoryProperties
    {
        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public void SaveCart_produces_same_repository_state_as_direct_Save(Product[] products, int qty)
        {
            var cartForService = new ShoppingCart();
            var cartForDirect = new ShoppingCart();

            foreach (var p in products)
            {
                cartForService.Add(p, qty);
                cartForDirect.Add(p, qty);

                var repoFromService = new InMemoryCartRepository();
                var service = new CartService(repoFromService);
                service.SaveCart(cartForService);

                var repoDirect = new InMemoryCartRepository();
                repoDirect.Save(cartForDirect);

                repoFromService.All().Count.Should().Be(repoDirect.All().Count);
                repoFromService.All().Single().TotalPrice().Should().Be(repoDirect.All().Single().TotalPrice());
            }
        }
    }

}
