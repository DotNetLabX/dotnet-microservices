using Auth.Grpc;
using Blocks.Core;
using Review.Domain.Shared.ValueObjects;
using Review.Domain.Reviewers.Events;

namespace Review.Domain.Reviewers;

public partial class Reviewer
{
		public static Reviewer Create(PersonInfo personInfo, IEnumerable<int> journalIds, IArticleAction action)
		{
				var reviewer = new Reviewer
				{
						Id = personInfo.Id,
						UserId = personInfo.UserId,
						Email = EmailAddress.Create(personInfo.Email),
						FirstName = personInfo.FirstName,
						LastName = personInfo.LastName,
						Honorific = personInfo.Honorific,
						Affiliation = personInfo.ProfessionalProfile?.Affiliation ?? string.Empty,
						CreatedById = action.CreatedById,
						CreatedOn = DateTime.UtcNow
				};

				if(journalIds.IsNotNullOrEmpty())
						reviewer._specializations 
								= [.. journalIds.Select(journalId => new ReviewerSpecialization { JournalId = journalId, ReviewerId = reviewer.Id })];

				var domainEvent = new ReviewerCreated(reviewer, action);
				reviewer.AddDomainEvent(domainEvent);

				return reviewer;
		}

		public void AddSpecialization(ReviewerSpecialization specialization)
		{
				if (!_specializations.Contains(specialization))
						_specializations.Add(specialization);
		}
}
