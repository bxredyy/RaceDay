// Handles events and categories and checks who owns them.
// References: Event rules: [6], [7]. Related records: [2], [10]. See docs/references.md.

using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Services
{
    public class EventService
    {
        private readonly RaceDayContext _db;
        private readonly AuditService _audit;
        public EventService(RaceDayContext db, AuditService audit) { _db = db; _audit = audit; }

        public Task<List<RaceEvent>> GetAll()
        {
            return _db.Events.AsNoTracking().OrderBy(race => race.EventDate).ToListAsync();
        }

        public async Task<RaceEvent> Get(int eventId)
        {
            return await _db.Events.FindAsync(eventId) ?? throw new ApiException(404, "Event not found.");
        }

        public async Task<RaceEvent> RequireOwner(int eventId, int organiserId)
        {
            var race = await Get(eventId);
            if (race.OrganiserId != organiserId)
                throw new ApiException(403, "Only this event's Organiser can manage it.");
            return race;
        }

        public async Task<RaceEvent> Create(int organiserId, EventRequest request)
        {
            var race = new RaceEvent { OrganiserId = organiserId };
            CopyDetails(race, request);
            _db.Events.Add(race);
            _audit.Log(organiserId, "Event created: " + race.Name + ".");
            await _db.SaveChangesAsync();
            return race;
        }

        public async Task<RaceEvent> Update(int eventId, int organiserId, EventRequest request)
        {
            var race = await RequireOwner(eventId, organiserId);
            CopyDetails(race, request);
            _audit.Log(organiserId, "Event updated: " + eventId + ".");
            await _db.SaveChangesAsync();
            return race;
        }

        public async Task Delete(int eventId, int organiserId)
        {
            var race = await RequireOwner(eventId, organiserId);
            if (await _db.Enrolments.AnyAsync(entry => entry.EventId == eventId))
                throw new ApiException(409, "This event has enrolments and cannot be deleted.");

            var categories = await _db.Categories.Where(category => category.EventId == eventId).ToListAsync();
            _db.Categories.RemoveRange(categories);
            _db.Events.Remove(race);
            _audit.Log(organiserId, "Event deleted: " + eventId + ".");
            await _db.SaveChangesAsync();
        }

        public async Task<List<Category>> GetCategories(int eventId)
        {
            await Get(eventId);
            return await _db.Categories.AsNoTracking().Where(category => category.EventId == eventId)
                .OrderBy(category => category.CategoryName).ToListAsync();
        }

        public async Task<Category> CreateCategory(int eventId, int organiserId, CategoryRequest request)
        {
            await RequireOwner(eventId, organiserId);
            await CheckCategoryName(eventId, request.CategoryName.Trim(), null);
            var category = new Category { EventId = eventId };
            CopyCategory(category, request);
            _db.Categories.Add(category);
            _audit.Log(organiserId, "Category created for event " + eventId + ".");
            await _db.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateCategory(int eventId, int categoryId, int organiserId, CategoryRequest request)
        {
            await RequireOwner(eventId, organiserId);
            var category = await FindCategory(eventId, categoryId);
            await CheckCategoryName(eventId, request.CategoryName.Trim(), categoryId);
            CopyCategory(category, request);
            _audit.Log(organiserId, "Category updated: " + categoryId + ".");
            await _db.SaveChangesAsync();
            return category;
        }

        public async Task DeleteCategory(int eventId, int categoryId, int organiserId)
        {
            await RequireOwner(eventId, organiserId);
            var category = await FindCategory(eventId, categoryId);
            if (await _db.Enrolments.AnyAsync(entry => entry.CategoryId == categoryId))
                throw new ApiException(409, "This category has enrolments and cannot be deleted.");
            _db.Categories.Remove(category);
            _audit.Log(organiserId, "Category deleted: " + categoryId + ".");
            await _db.SaveChangesAsync();
        }

        private async Task<Category> FindCategory(int eventId, int categoryId)
        {
            return await _db.Categories.SingleOrDefaultAsync(category => category.EventId == eventId && category.CategoryId == categoryId)
                ?? throw new ApiException(404, "Category not found for this event.");
        }

        private async Task CheckCategoryName(int eventId, string name, int? excludingId)
        {
            if (await _db.Categories.AnyAsync(category => category.EventId == eventId && category.CategoryName == name && category.CategoryId != excludingId))
                throw new ApiException(409, "A category with this name already exists for this event.");
        }

        private static void CopyDetails(RaceEvent race, EventRequest request)
        {
            if (!request.EventDate.HasValue || request.EventDate.Value.Year < 2000)
                throw new ApiException(400, "Provide a valid event date from 2000 onwards.");
            if (decimal.Round(request.DistanceKm, 2) != request.DistanceKm)
                throw new ApiException(400, "Distance accepts at most two decimal places.");
            race.Name = request.Name.Trim();
            race.Description = request.Description.Trim();
            race.EventDate = request.EventDate.Value.ToUniversalTime();
            race.Location = request.Location.Trim();
            race.DistanceKm = request.DistanceKm;
            race.EventType = request.EventType;
            race.BannerImageUrl = request.BannerImageUrl;
        }

        private static void CopyCategory(Category category, CategoryRequest request)
        {
            if (decimal.Round(request.Fee, 2) != request.Fee || decimal.Round(request.DistanceKm, 2) != request.DistanceKm)
                throw new ApiException(400, "Fee and distance accept at most two decimal places.");
            category.CategoryName = request.CategoryName.Trim();
            category.Description = request.Description.Trim();
            category.DistanceKm = request.DistanceKm;
            category.Fee = request.Fee;
        }
    }
}
