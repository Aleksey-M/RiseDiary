using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Model
{
    public interface ICogitationsService
    {
        Task<List<Cogitation>> GetRecordCogitations(Guid recordId);

        Task<Guid> AddCogitation(Guid recordId, string cogitationText);

        Task UpdateCogitationText(Guid cogitationId, string newText);

        Task DeleteCogitation(Guid cogitationId);
    }
}
