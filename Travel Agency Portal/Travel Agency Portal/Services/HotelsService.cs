using Travel_Agency_Portal.Data;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Services;

public class HotelsService : OperationService
{
    private readonly HotelsRepository _repository;
    private readonly CatalogCache _catalogCache;

    public HotelsService(HotelsRepository repository, CatalogCache catalogCache)
    {
        _repository = repository;
        _catalogCache = catalogCache;
    }

    public OperationResult GetHotels(int? placeId = null)
    {
        return Ok(_repository.List(placeId));
    }

    public OperationResult AddHotel(Hotel hotel)
    {
        var id = _repository.Create(hotel);

        _catalogCache.Invalidate();
        return new OperationResult(201, new { id, message = "Hotel added successfully." }, $"/api/v1/hotels");
    }

    public OperationResult UpdateHotel(int id, Hotel hotel)
    {
        var rows = _repository.Update(id, hotel);

        _catalogCache.Invalidate();
        return rows > 0 ? Ok(new { message = "Hotel updated successfully." }) : NotFound();
    }

    public OperationResult DeleteHotel(int id)
    {
        if (_repository.CountPackages(id) > 0)
        {
            return Conflict(new { message = "This hotel is used by packages. Delete those packages first." });
        }

        var rows = _repository.Delete(id);

        _catalogCache.Invalidate();
        return rows > 0 ? Ok(new { message = "Hotel deleted successfully." }) : NotFound();
    }

}
