using System.Linq.Expressions;
using AutoMapper;
using CleanArchitecture.Application;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.Book;
using Moq;

namespace CleanArchitecture.Unittest.TestCase;

public class BookServiceTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private BookService _bookService;

    [Fact]
    public async Task BookService_GetById_ShouldReturnABook()
    {
        // Arrange
        int bookId = 1;
        var bookDTO = new BookDTO
        {
            Id = 1,
            Title = "C# Programming",
            Description = "A comprehensive guide to C# programming.",
            Price = 29.99
        };
        var expect = new Book
        {
            Id = 1,
            Title = "C# Programming",
            Description = "A comprehensive guide to C# programming.",
            Price = 29.99
        };
        _unitOfWorkMock.Setup(u => u.BookRepository.FirstOrDefaultAsync(b => b.Id == bookId, null))
                       .ReturnsAsync(expect);

        _mapperMock.Setup(m => m.Map<BookDTO>(expect)).Returns(bookDTO);
        _bookService = new BookService(_unitOfWorkMock.Object, _mapperMock.Object);

        // Act
        var actualResult = await _bookService.Get(bookId);

        // Assert
        Assert.Equal(expect.Id, actualResult.Id);
        Assert.Equal(expect.Title, actualResult.Title);
        Assert.Equal(expect.Description, actualResult.Description);
        Assert.Equal(expect.Price, actualResult.Price);
    }

    [Fact]
    public async Task BookService_Get_ShouldReturnBooks()
    {
        // Arrange
        var expectedBooks = new List<BookDTO>
        {
            new BookDTO
            {
                Id = 1,
                Title = "C# Programming",
                Description = "A comprehensive guide to C# programming.",
                Price = 29.99
            },
            new BookDTO
            {
                Id = 2,
                Title = "ASP.NET Core Development",
                Description = "Learn how to build web applications using ASP.NET Core.",
                Price = 35.50
            }
        };

        var expectedResult = new Pagination<BookDTO>(expectedBooks, expectedBooks.Count, 0, 2);

        _unitOfWorkMock
            .Setup(u => u.BookRepository.ToPagination(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Book, bool>>>(),
                It.IsAny<Func<IQueryable<Book>, IQueryable<Book>>>(),
                It.IsAny<Expression<Func<Book, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<Book, BookDTO>>>()))
            .ReturnsAsync(expectedResult);


        _bookService = new BookService(_unitOfWorkMock.Object, _mapperMock.Object);

        // Act
        var actualResult = await _bookService.Get(0, 2);

        // Assert
        Assert.NotNull(actualResult);
        Assert.Equal(expectedResult.CurrentPage, actualResult.CurrentPage);
        Assert.Equal(expectedResult.TotalPages, actualResult.TotalPages);
        Assert.Equal(expectedResult.PageSize, actualResult.PageSize);
        Assert.Equal(expectedResult.TotalCount, actualResult.TotalCount);
        Assert.Equal(expectedResult.HasPrevious, actualResult.HasPrevious);
        Assert.Equal(expectedResult.HasNext, actualResult.HasNext);
        Assert.Equal(expectedResult.Items?.Count, actualResult.Items?.Count);


        var expect = expectedResult.Items?.ToList();
        var actual = actualResult.Items?.ToList();

        for (int i = 0; i < expectedResult.Items?.Count; i++)
        {
            Assert.Equal(expect?[i].Id, actual?[i].Id);
            Assert.Equal(expect?[i].Title, actual?[i].Title);
            Assert.Equal(expect?[i].Description, actual?[i].Description);
            Assert.Equal(expect?[i].Price, actual?[i].Price);
        }
    }

    [Fact]
    public async Task BookService_Add_ShouldAddBook()
    {
        // Arrange
        var expect = new Book
        {
            Id = 1,
            Title = "New Book",
            Description = "A new book description.",
            Price = 19.99
        };

        _mapperMock.Setup(m => m.Map<BookDTO>(expect)).Returns(new BookDTO
        {
            Id = 1,
            Title = "New Book",
            Description = "A new book description.",
            Price = 19.99
        });

        _unitOfWorkMock.Setup(u => u.BookRepository.AddAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);

        _bookService = new BookService(_unitOfWorkMock.Object, _mapperMock.Object);

        // Act
        var result = await _bookService.Add(new AddBookRequest
        {
            Title = "New Book",
            Description = "A new book description.",
            Price = 19.99
        }, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), CancellationToken.None), Times.Once);
    }


    [Fact]
    public async Task BookService_Update_ShouldUpdateBook()
    {
        // Arrange
        var updateRequest = new UpdateBookRequest
        {
            Id = 1,
            Title = "Updated Book",
            Description = "An updated book description.",
            Price = 30.00
        };

        _unitOfWorkMock.Setup(u => u.BookRepository.AnyAsync(
            It.IsAny<Expression<Func<Book, bool>>>()))
                       .ReturnsAsync(true);

        _bookService = new BookService(_unitOfWorkMock.Object, _mapperMock.Object);

        // Act
        await _bookService.Update(updateRequest, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.BookRepository.AnyAsync(
            It.IsAny<Expression<Func<Book, bool>>>()), Times.Once);
    }

    [Fact]
    public async Task BookService_Delete_ShouldRemoveBook()
    {
        // Arrange
        var bookId = 1;
        var bookToDelete = new Book
        {
            Id = bookId,
            Title = "Book to Delete",
            Description = "A book to delete",
            Price = 20.00
        };

        _unitOfWorkMock.Setup(u => u.BookRepository.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Book, bool>>>(),
            It.IsAny<Func<IQueryable<Book>, IQueryable<Book>>>()))
                       .ReturnsAsync(bookToDelete);

        _unitOfWorkMock.Setup(u => u.BookRepository.Delete(It.IsAny<Book>())).Callback<Book>(book => { });

        // Initialize the BookService with the mocked unit of work
        _bookService = new BookService(_unitOfWorkMock.Object, _mapperMock.Object);

        // Act
        await _bookService.Delete(bookId, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.BookRepository.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Book, bool>>>(),
            It.IsAny<Func<IQueryable<Book>, IQueryable<Book>>>()), Times.Once);
    }


    [Fact]
    public async Task BookService_GetById_InvalidId_ShouldReturnNull()
    {
        // Arrange
        int invalidBookId = -1;
        _unitOfWorkMock.Setup(u => u.BookRepository.FirstOrDefaultAsync(b => b.Id == invalidBookId, null))
                       .ReturnsAsync((Book)null);

        _bookService = new BookService(_unitOfWorkMock.Object, _mapperMock.Object);

        // Act
        var result = await _bookService.Get(invalidBookId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task BookService_Add_InvalidData_ShouldThrowException()
    {
        // Arrange
        var invalidBookRequest = new AddBookRequest
        {
            Title = "", // Empty title
            Description = new string('A', 10000), // Large string but within practical limits
            Price = -10 // Negative price
        };

        _bookService = new BookService(_unitOfWorkMock.Object, _mapperMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _bookService.Add(invalidBookRequest, CancellationToken.None));
    }

    [Fact]
    public async Task BookService_Update_NonExistentBook_ShouldThrowNotFoundException()
    {
        // Arrange
        var updateRequest = new UpdateBookRequest
        {
            Id = 9999, // Non-existent ID
            Title = "Updated Book",
            Description = new string('B', 10000), // Large string but within practical limits
            Price = 25.00
        };
        _unitOfWorkMock.Setup(u => u.BookRepository.AnyAsync(It.IsAny<Expression<Func<Book, bool>>>()))
                       .ReturnsAsync(false);

        _bookService = new BookService(_unitOfWorkMock.Object, _mapperMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _bookService.Update(updateRequest, CancellationToken.None));
    }

    [Fact]
    public async Task BookService_Delete_NonExistentBook_ShouldThrowNotFoundException()
    {
        // Arrange
        int nonExistentBookId = 9999;
        _unitOfWorkMock.Setup(u => u.BookRepository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Book, bool>>>(), null))
                       .ReturnsAsync((Book)null);
        _bookService = new BookService(_unitOfWorkMock.Object, _mapperMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _bookService.Delete(nonExistentBookId, CancellationToken.None));
    }

    [Fact]
    public async Task BookService_GetById_ZeroId_ShouldReturnNull()
    {
        // Arrange
        int zeroBookId = 0;
        _unitOfWorkMock.Setup(u => u.BookRepository.FirstOrDefaultAsync(b => b.Id == zeroBookId, null))
                       .ReturnsAsync((Book)null);

        _bookService = new BookService(_unitOfWorkMock.Object, _mapperMock.Object);

        // Act
        var result = await _bookService.Get(zeroBookId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task BookService_Update_EmptyTitle_ShouldThrowException()
    {
        // Arrange
        var updateRequest = new UpdateBookRequest
        {
            Id = 1,
            Title = "", // Empty title
            Description = "Valid Description",
            Price = 30.00
        };

        _bookService = new BookService(_unitOfWorkMock.Object, _mapperMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _bookService.Update(updateRequest, CancellationToken.None));
    }

    [Fact]
    public async Task BookService_Add_MaxLengthTitle_ShouldAddSuccessfully()
    {
        // Arrange
        var validBookRequest = new AddBookRequest
        {
            Title = new string('C', 4000), // Large string within NVARCHAR(MAX) practical limit
            Description = "Valid Description",
            Price = 50.00
        };

        _unitOfWorkMock.Setup(u => u.BookRepository.AddAsync(It.IsAny<Book>()))
                       .Returns(Task.CompletedTask);

        _bookService = new BookService(_unitOfWorkMock.Object, _mapperMock.Object);

        // Act
        await _bookService.Add(validBookRequest, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.BookRepository.AddAsync(It.Is<Book>(b =>
            b.Title == validBookRequest.Title &&
            b.Description == validBookRequest.Description &&
            b.Price == validBookRequest.Price)), Times.Once);
        _unitOfWorkMock.Verify(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), CancellationToken.None), Times.Once);
    }

}
