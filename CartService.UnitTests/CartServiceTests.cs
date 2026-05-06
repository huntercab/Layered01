using Moq;
using Xunit;
using CartService.Business.Services;
using CartService.DataAccess.Interfaces;
using CartService.Domain.Entities;
using CartService.Domain.ValueObjects;

namespace CartService.UnitTests
{
    public class CartServiceTests
    {
        private readonly Mock<ICartRepository> _cartRepositoryMock;
        private readonly Business.Services.CartService _cartService;

        public CartServiceTests() 
        { 
            _cartRepositoryMock = new Mock<ICartRepository>();
            _cartService = new Business.Services.CartService(_cartRepositoryMock.Object);
        }

        [Fact]
        public async Task GetItemsAsync_ShouldReturnEmptyList_WhenCartDoesntExist()
        {
            //AAA
            var cartId = Guid.NewGuid();

            _cartRepositoryMock
                .Setup(x => x.GetByIdAsync(cartId))
                .ReturnsAsync((Cart?)null);

            var result = await _cartService.GetCartItemsAsync(cartId);

            Assert.Empty(result);
        }

        [Fact]
        public async Task AddItemSync_ShouldCreateCart_WhenCartDoesntExist()
        {
            var cartId = Guid.NewGuid();
            var item = CreateCartItem();

            _cartRepositoryMock
                .Setup(x => x.GetByIdAsync(cartId))
                .ReturnsAsync((Cart?)null);

            await _cartService.AddItemAsync(cartId, item);

            _cartRepositoryMock.Verify(x => x.SaveAsync(
                It.Is<Cart>(cart =>
                        cart.Id == cartId &&
                        cart.Items.Count == 1 &&
                        cart.Items.First().Id == item.Id
                    )
                ), Times.Once);
        }

        [Fact]
        public async Task AddItemAsync_ShouldIncreaseQuantity_WhenItemAlreadyExist()
        {
            var cartId = Guid.NewGuid();

            var cart = new Cart(cartId);
            cart.AddItem(CreateCartItem(quantity: 2));

            var newItem = CreateCartItem(quantity: 3);

            _cartRepositoryMock
                .Setup(x => x.GetByIdAsync(cartId))
                .ReturnsAsync(cart);

            await _cartService.AddItemAsync(cartId, newItem);

            _cartRepositoryMock.Verify(x => x.SaveAsync(
                It.Is<Cart>(cart =>
                        cart.Items.Count == 1 &&
                        cart.Items.First().Quantity == 5
                    )
                ), Times.Once);
        }

        [Fact]
        public async Task RemoveItemAsync_ShouldRemoveItem_WhenCartExist()
        {
            var cartId = Guid.NewGuid();

            var cart = new Cart(cartId);
            cart.AddItem(CreateCartItem(id: 1));
            cart.AddItem(CreateCartItem(id: 2));

            _cartRepositoryMock
                .Setup(x => x.GetByIdAsync(cartId))
                .ReturnsAsync(cart);

            await _cartService.RemoveItemAsync(cartId, 1);

            _cartRepositoryMock.Verify(x => x.SaveAsync(
                It.Is<Cart>(cart =>
                        cart.Items.Count == 1 &&
                        cart.Items.First().Id == 2
                    )
                ), Times.Once);
        }

        [Fact]
        public async Task RemoveItemAsync_ShouldNotSave_WhenCartDoesntExist()
        {
            var cartId = Guid.NewGuid();

            _cartRepositoryMock
                .Setup(x => x.GetByIdAsync(cartId))
                .ReturnsAsync((Cart?)null);

            await _cartService.RemoveItemAsync(cartId, 1);

            _cartRepositoryMock.Verify(x => x.SaveAsync(
                It.IsAny<Cart>()), Times.Never);
        }

        [Fact]
        public async Task AddItemAsync_ShouldThrowException_WhenCartIdIsEmpty()
        {
            var item = CreateCartItem();

            await Assert.ThrowsAsync<ArgumentException>(() => 
                _cartService.AddItemAsync(Guid.Empty, item));
        }

        private static CartItem CreateCartItem(
            int id = 1,
            string name = "book1",
            decimal amount = 25,
            string currency = "USD",
            int quantity = 1
            )
        {
            return new CartItem(
                id: id,
                name: name,
                price: Money.Create(amount,currency),
                quantity: quantity
                );
        }

    }
}
