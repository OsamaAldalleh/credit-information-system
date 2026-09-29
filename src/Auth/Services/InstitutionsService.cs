using Auth.Data;
using Auth.Errors;
using Auth.Mappers;
using Auth.Payloads;
using Common.Exceptions;
using EntityFramework.Exceptions.Common;

namespace Auth.Services;

public class InstitutionsService(AuthDbContext authDb)
{
    public async Task<InstitutionPayload> CreateInstitutionAsync(CreateInstitutionPayload payload)
    {
        var institution = payload.ToEntity();
        authDb.Institutions.Add(institution);

        try
        {
            await authDb.SaveChangesAsync();
        }
        catch (UniqueConstraintException)
        {
            throw ServiceException.BadRequest(AuthErrors.InstitutionAlreadyExists, institution.Name);
        }

        return institution.ToPayload();
    }
}
