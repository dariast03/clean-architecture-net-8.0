using AutoMapper;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Shared.Models;
using CleanArchitecture.Shared.Models.Book;

namespace CleanArchitecture.Application.Services;

public class BookService(IUnitOfWork unitOfWork, IMapper mapper) : IBookService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Pagination<BookDTO>> Get(int pageIndex, int pageSize)
    {
        var books = await _unitOfWork.BookRepository.ToPagination(
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: x => x.Title,
            ascending: true,
            selector: x => new BookDTO
            {
                Title = x.Title,
                Price = x.Price,
                Description = x.Description
            }
        );

        return books;
    }

    public async Task<BookDTO> Get(int id)
    {
        var book = await _unitOfWork.BookRepository.FirstOrDefaultAsync(x => x.Id == id);
        return _mapper.Map<BookDTO>(book);
    }

    public async Task<BookDTO> Add(AddBookRequest request, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title cannot be empty or whitespace.", nameof(request.Title));
        if (request.Price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(request.Price));

        var book = _mapper.Map<Book>(request);
        await _unitOfWork.ExecuteTransactionAsync(async () => await _unitOfWork.BookRepository.AddAsync(book), token);
        return _mapper.Map<BookDTO>(book);
    }

    public async Task<BookDTO> Update(UpdateBookRequest request, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title cannot be empty or whitespace.", nameof(request.Title));

        if (!await _unitOfWork.BookRepository.AnyAsync(x => x.Id == request.Id))
            throw new KeyNotFoundException("Book not found");

        var book = _mapper.Map<Book>(request);
        await _unitOfWork.ExecuteTransactionAsync(() => _unitOfWork.BookRepository.Update(book), token);
        return _mapper.Map<BookDTO>(book);
    }

    public async Task<BookDTO> Delete(int id, CancellationToken token)
    {
        var existBook = await _unitOfWork.BookRepository.FirstOrDefaultAsync(x => x.Id == id);
        if (existBook == null)
        {
            throw new KeyNotFoundException("Book not found");
        }

        await _unitOfWork.ExecuteTransactionAsync(() => _unitOfWork.BookRepository.Delete(existBook), token);
        return _mapper.Map<BookDTO>(existBook);
    }
}
