using Travel_Agency_Portal.Data;
using Travel_Agency_Portal.Dtos;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Services;

public class PackagesService : OperationService
{
    private readonly PackagesRepository _repository;

    private readonly CatalogCache _catalogCache;

    public PackagesService(PackagesRepository repository, CatalogCache catalogCache)
    {
        _repository = repository;
        _catalogCache = catalogCache;
    }

    public OperationResult GetPackages(string? search = null, int? placeId = null, decimal? minPrice = null, decimal? maxPrice = null)
    {
        var cacheKey = $"packages:{search}:{placeId}:{minPrice}:{maxPrice}";
        var packages = _catalogCache.GetOrCreate(cacheKey, () => _repository.List(search, placeId, minPrice, maxPrice));
        return Ok(packages);
    }

    public OperationResult GetPackage(int id)
    {
        var cacheKey = $"packages:id:{id}";
        var result = _catalogCache.GetOrCreate(cacheKey, () => _repository.Find(id));
        if (result.Count == 0)
        {
            return NotFound();
        }

        return Ok(result[0]);
    }

    public OperationResult AddPackage(PackageRequestDto package)
    {
        if (package.End_Date.Date < package.Start_Date.Date)
        {
            return BadRequest(new { message = "End date must be after start date." });
        }

        if (!_repository.HotelBelongsToPlace(package.Hotel_Id, package.Place_Id))
        {
            return BadRequest(new { message = "Selected hotel does not belong to this destination." });
        }

        var id = _repository.Create(package);

        _catalogCache.Invalidate();
        return new OperationResult(201, new { id, message = "Package added successfully." }, $"/api/v1/packages/{id}");
    }

    public OperationResult UpdatePackage(int id, PackageRequestDto package)
    {
        if (package.End_Date.Date < package.Start_Date.Date)
        {
            return BadRequest(new { message = "End date must be after start date." });
        }

        if (!_repository.HotelBelongsToPlace(package.Hotel_Id, package.Place_Id))
        {
            return BadRequest(new { message = "Selected hotel does not belong to this destination." });
        }

        var rows = _repository.Update(id, package);

        _catalogCache.Invalidate();
        return rows > 0 ? Ok(new { message = "Package updated successfully." }) : NotFound();
    }

    public OperationResult DeletePackage(int id)
    {
        if (_repository.CountBookings(id) > 0)
        {
            return Conflict(new { message = "This package has bookings and cannot be deleted." });
        }

        var rows = _repository.Delete(id);

        _catalogCache.Invalidate();
        return rows > 0 ? Ok(new { message = "Package deleted successfully." }) : NotFound();
    }

}
