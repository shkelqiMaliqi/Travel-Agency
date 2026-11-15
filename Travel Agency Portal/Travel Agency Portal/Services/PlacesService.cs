using Travel_Agency_Portal.Data;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Services;

public class PlacesService : OperationService
{
    private readonly PlacesRepository _repository;

    private readonly CatalogCache _catalogCache;

    public PlacesService(PlacesRepository repository, CatalogCache catalogCache)
    {
        _repository = repository;
        _catalogCache = catalogCache;
    }

    public OperationResult GetPlaces(string? search = null)
    {
        var cacheKey = $"places:{search?.Trim().ToLowerInvariant()}";
        var places = _catalogCache.GetOrCreate(cacheKey, () => _repository.List(search));
        return Ok(places);
    }

    public OperationResult GetPlace(int id)
    {
        var cacheKey = $"places:id:{id}";
        var result = _catalogCache.GetOrCreate(cacheKey, () => _repository.Find(id));

        if (result.Count == 0)
        {
            return NotFound();
        }

        return Ok(result[0]);
    }

    public OperationResult AddPlace(Place place)
    {
        var id = _repository.Create(place);

        _catalogCache.Invalidate();
        return new OperationResult(201, new { id, message = "Place added successfully." }, $"/api/v1/places/{id}");
    }

    public OperationResult UpdatePlace(int id, Place place)
    {
        var rows = _repository.Update(id, place);

        _catalogCache.Invalidate();
        return rows > 0 ? Ok(new { message = "Place updated successfully." }) : NotFound();
    }

    public OperationResult DeletePlace(int id)
    {
        if (_repository.CountDependencies(id) > 0)
        {
            return Conflict(new { message = "This destination has hotels or packages. Delete those first." });
        }

        var rows = _repository.Delete(id);

        _catalogCache.Invalidate();
        return rows > 0 ? Ok(new { message = "Place deleted successfully." }) : NotFound();
    }

}
