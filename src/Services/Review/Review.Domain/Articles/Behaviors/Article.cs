using Mapster;
using Auth.Grpc;
using Blocks.Domain;
using Articles.IntegrationEvents.Contracts.Articles.Dtos;
using Review.Domain.Articles.Events;
using Review.Domain.Assets;
using Review.Domain.Invitations;
using Review.Domain.Invitations.Events;
using Review.Domain.Invitations.ValueObjects;
using Review.Domain.Reviewers;
using Review.Domain.Reviewers.Events;
using Review.Domain.StateMachines;

namespace Review.Domain.Articles;

public partial class Article
{
		public void SetStage(ArticleStage newStage, ArticleStateMachineFactory stateMachineFactory, IArticleAction action)
    {
				if (newStage == Stage)
            return;

				stateMachineFactory.ValidateStageTransition(Stage, action.ActionType);

				var currentStage = Stage;
				Stage = newStage;
        LastModifiedOn = action.CreatedOn;
				LastModifiedById = action.CreatedById;

				_stageHistories.Add(
						new StageHistory { ArticleId = Id, StageId = newStage, StartDate = DateTime.UtcNow });
				
				AddDomainEvent(
						new ArticleStageChanged(currentStage, newStage, action));
    }

		public void AssignEditor(Editor actor, IArticleAction action)
		{
				if (_actors.Exists(a => a.PersonId == actor.Id && a.Role == UserRoleType.REVED))
						throw new DomainException($"Editor {actor.Email} is already assigned to the article");

				if (_actors.Exists(a => a.Role == UserRoleType.REVED))
						throw new DomainException($"An Editor is already assigned to the article");

				_actors.Add(new ArticleActor() { Person = actor, Role = UserRoleType.REVED });

				AddDomainEvent(
						new EditorAssigned(actor.Id, actor.UserId!.Value, action));
				
				AddAction(action);
		}

		public void AssignReviewer(Reviewer reviewer, IArticleAction action)
		{
				if (_actors.Exists(a => a.PersonId == reviewer.Id && a.Role == UserRoleType.REV))
						throw new DomainException($"Reviewer {reviewer.Email} is already assigned to the article");

				reviewer.AddSpecialization(new ReviewerSpecialization() { JournalId = this.JournalId, ReviewerId = reviewer.Id});

				_actors.Add(new ArticleActor() { Person = reviewer, Role = UserRoleType.REV });

				AddDomainEvent(
						new ReviewerAssigned(this, reviewer, action));

				AddAction(action);
		}

		public void AssignActor(int personId, UserRoleType roleType)
		{
				_actors.Add(new ArticleActor { ArticleId = this.Id, PersonId = personId, Role = roleType });
		}

		public void Accept(ArticleStateMachineFactory _stateMachineFactory, IArticleAction action)
		{
				SetStage(ArticleStage.Accepted, _stateMachineFactory, action);
				
				AddDomainEvent(new ArticleAccepted(this, action));
		}

		public void Reject(ArticleStateMachineFactory _stateMachineFactory, IArticleAction action)
		{
				SetStage(ArticleStage.Rejected, _stateMachineFactory, action);

				AddDomainEvent(new ArticleRejected(this, action));
		}

		public Asset CreateAsset(AssetTypeDefinition type, IArticleAction action)
		{
				var assetCount = _assets
						.Where(a => a.Type == type.Id)
						.Count();

				if (assetCount >= type.MaxAssetCount)
						throw new DomainException($"The maximum number of files, {type.MaxAssetCount}, allowed for {type.Name.ToString()} was already reached");

				var asset = Asset.Create(this, type, assetCount, action);
				_assets.Add(asset);

				AddAction(action);
				return asset;
		}

		public ReviewInvitation InviteReviewer(Reviewer reviewer, IArticleAction action)
		{
				if (!reviewer.Specializations.Any(s => s.JournalId == this.JournalId))
						throw new DomainException($"Reviewer {reviewer.FullName} is not specialized in article's journal");

				return CreateInvitation(reviewer.UserId, reviewer.Email.Value, reviewer.FirstName, reviewer.LastName, action: action);
		}

		public ReviewInvitation InviteReviewer(int? userId, string email, string firstName, string lastName, IArticleAction action)
		{
				return CreateInvitation(userId, email, firstName, lastName, action);
		}

		public ReviewInvitation InviteReviewer(PersonInfo personInfo, IArticleAction action)
		{
				return CreateInvitation(personInfo.UserId, personInfo.Email, personInfo.FirstName, personInfo.LastName, action);
		}

		private ReviewInvitation CreateInvitation(int? userId, string email, string firstName, string lastName, IArticleAction action)
		{
				// check if there is an active invitation for this email
				if (_invitations.Any(i =>
								i.Email.Value.Trim().ToUpperInvariant() == email.Trim().ToUpperInvariant()
								&& !i.IsExpired)) 
						throw new DomainException($"Reviewer {firstName} {lastName} ({email}) was already invited");

				var invitation = new ReviewInvitation
				{
						ArticleId = this.Id,
						UserId = userId,
						Email = email,
						FirstName = firstName,
						LastName = lastName,
						SentById = action.CreatedById,
						ExpiresOn = DateTime.UtcNow.AddDays(7),
						Token = InvitationToken.CreateNew(),
				};

				AddDomainEvent(new ReviewerInvited(invitation, action));

				_invitations.Add(invitation);
				return invitation;
		}

		private void AddAction(IArticleAction action)
		{
				_actions.Add(action.Adapt<ArticleAction>());
				AddDomainEvent(new ArticleActionExecuted(this, action));
		}

		public static Article FromSubmission(ArticleDto articleDto, IEnumerable<ArticleActor> actors, IEnumerable<Asset> assets, ArticleStateMachineFactory stateMachineFactory, IArticleAction action)
		{
				var article = new Article
				{
						Id = articleDto.Id,
						JournalId = articleDto.Journal.Id,
						Title = articleDto.Title,
						Type = articleDto.Type,
						Scope = articleDto.Scope,
						SubmittedById = articleDto.SubmittedBy.Id,
						SubmittedOn = articleDto.SubmittedOn,
						Stage = articleDto.Stage						
				};
				article._actors.AddRange(actors);
				article._assets.AddRange(assets);

				article.SetStage(ArticleStage.UnderReview, stateMachineFactory, action);

				return article;
		}
}
