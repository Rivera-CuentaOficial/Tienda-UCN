using Mapster;
using Serilog;
using TiendaUCN.src.Application.DTOs.ProductResponse;
using TiendaUCN.src.Application.DTOs.ProductResponse.Admin;
using TiendaUCN.src.Application.DTOs.ProductResponse.Customer;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IConfiguration _configuration;
    private readonly IFileService _fileService;
    private readonly int _defaultPageSize;

    public ProductService(IProductRepository productRepository, IConfiguration configuration, IFileService fileService)
    {
        _productRepository = productRepository;
        _configuration = configuration;
        _fileService = fileService;
        _defaultPageSize =
            _configuration.GetValue<int?>("Products:DefaultPageSize")
            ?? throw new ArgumentNullException(
                "La configuración 'DefaultPageSize' no está definida."
            );
    }

    public async Task<ListedProductsForAdminDTO> GetFilteredForAdminAsync(
        SearchParamsDTO searchParams
    )
    {
        Log.Information(
            "Obteniendo productos para administrador con parámetros de búsqueda: {@SearchParams}",
            searchParams
        );
        var (products, totalCount) = await _productRepository.GetFilteredForAdminAsync(
            searchParams
        );
        var totalPages = (int)
            Math.Ceiling((double)totalCount / (searchParams.PageSize ?? _defaultPageSize));
        int currentPage = searchParams.PageNumber;
        int pageSize = searchParams.PageSize ?? _defaultPageSize;
        if (currentPage < 1 || currentPage > totalPages)
        {
            throw new ArgumentOutOfRangeException("El número de página está fuera de rango.");
        }
        Log.Information(
            "Total de productos encontrados: {TotalCount}, Total de páginas: {TotalPages}, Página actual: {CurrentPage}, Tamaño de página: {PageSize}",
            totalCount,
            totalPages,
            currentPage,
            pageSize
        );

        // Convertimos los productos filtrados a DTOs para la respuesta
        return new ListedProductsForAdminDTO
        {
            Products = products.Adapt<List<ProductForAdminDTO>>(),
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = currentPage,
            PageSize = products.Count(),
        };
    }

    public async Task<ListedProductsForCustomerDTO> GetFilteredForCustomerAsync(
        SearchParamsDTO searchParams
    )
    {
        var (products, totalCount) = await _productRepository.GetFilteredForCustomerAsync(
            searchParams
        );
        var totalPages = (int)
            Math.Ceiling((double)totalCount / (searchParams.PageSize ?? _defaultPageSize));
        int currentPage = searchParams.PageNumber;
        int pageSize = searchParams.PageSize ?? _defaultPageSize;
        if (currentPage < 1 || currentPage > totalPages)
        {
            throw new ArgumentOutOfRangeException("El número de página está fuera de rango.");
        }
        Log.Information(
            "Total de productos encontrados: {TotalCount}, Total de páginas: {TotalPages}, Página actual: {CurrentPage}, Tamaño de página: {PageSize}",
            totalCount,
            totalPages,
            currentPage,
            pageSize
        );

        // Convertimos los productos filtrados a DTOs para la respuesta
        return new ListedProductsForCustomerDTO
        {
            Products = products.Adapt<List<ProductForCustomerDTO>>(),
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = currentPage,
            PageSize = products.Count(),
        };
    }

    public async Task<ProductDetailDTO> GetByIdAsync(int id)
    {
        var product =
            await _productRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");
        Log.Information("Producto encontrado: {@Product}", product);
        return product.Adapt<ProductDetailDTO>();
    }

    public async Task<ProductDetailDTO> GetByIdForAdminAsync(int id)
    {
        var product =
            await _productRepository.GetByIdForAdminAsync(id)
            ?? throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");
        Log.Information("Producto encontrado: {@Product}", product);
        return product.Adapt<ProductDetailDTO>();
    }

    public async Task<string> CreateAsync(CreateProductDTO createProductDTO)
    {
        Product product = createProductDTO.Adapt<Product>();
        Category category =
            await _productRepository.CreateOrGetCategoryAsync(createProductDTO.CategoryName)
            ?? throw new Exception("Error al crear o obtener la categoría del producto.");
        Brand brand =
            await _productRepository.CreateOrGetBrandAsync(createProductDTO.BrandName)
            ?? throw new Exception("Error al crear o obtener la marca del producto.");
        product.CategoryId = category.Id;
        product.BrandId = brand.Id;
        product.Images = new List<Image>();
        int productId = await _productRepository.CreateAsync(product);
        Log.Information("Producto creado: {@Product}", product);

        // Si se enviaron imágenes en el DTO las subimos a Cloudinary y las asociamos al producto
        if (createProductDTO.Images == null || !createProductDTO.Images.Any())
        {
            Log.Information("No se proporcionaron imágenes. No se subirán imágenes al crear el producto.");
        }
        else
        {
            foreach (var image in createProductDTO.Images)
            {
                try
                {
                    Log.Information("Subiendo imagen asociada al producto: {FileName}", image.FileName);
                    await _fileService.UploadAsync(image, productId);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error subiendo la imagen {FileName} para el producto {ProductId}", image.FileName, productId);
                    throw; // Dejar que el middleware de excepciones maneje la respuesta
                }
            }
        }

        return product.Id.ToString();
    }

    public async Task ToggleActiveAsync(int id)
    {
        await _productRepository.ToggleActiveAsync(id);
    }
}