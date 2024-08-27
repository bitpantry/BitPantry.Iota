using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Application.Parsers.BibleData;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BitPantry.Iota.Application.CRQS.Bible.Command
{
    public class InstallBibleCommandHandler : IRequestHandler<InstallBibleCommand, long>
    {
        private EntityDataContext _dbCtx;

        public InstallBibleCommandHandler(EntityDataContext dbCtx) { _dbCtx = dbCtx; }

        public async Task<long> Handle(InstallBibleCommand request, CancellationToken cancellationToken)
        {
            var newBible = new DefaultXmlBibleDataParser().Parse(request.FilePathToBibleData);
            _dbCtx.Bibles.Add(newBible);
            await _dbCtx.SaveChangesAsync();

            return newBible.Id;
        }
    }

    public record InstallBibleCommand(string FilePathToBibleData) : IRequest<long> { }

}
