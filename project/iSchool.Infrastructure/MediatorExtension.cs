using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Infrastructure
{
    public static class MediatorExtension
    {
        public static async Task DispatchDomainEventsAsync(this IMediator mediator, IEnumerable<Entity> domainEntities)
        {
            var domainEvents = domainEntities
                .Where(x => x.DomainEvents != null && x.DomainEvents.Any())
                .SelectMany(x => x.DomainEvents)
                .ToList();

            domainEntities.ToList()
                .ForEach(entity => entity.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
                await mediator.Publish(domainEvent);
        }

        public static async Task DispatchDomainEventsAsync(this IMediator mediator, Entity domainEntitie)
        {
            if (domainEntitie.DomainEvents.Any())
            {
                var domainEvents = domainEntitie.DomainEvents.ToList();
                domainEntitie.ClearDomainEvents();
                foreach (var domainEvent in domainEvents)
                    await mediator.Publish(domainEvent);
            }
          


        }
    }
}
