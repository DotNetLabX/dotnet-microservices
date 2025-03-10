using Articles.Abstractions.Enums;
using Blocks.Core;
using Blocks.Core.Security;

namespace ArticleTimeline.Application.VariableResolvers;

public interface IVariableResolver
{
    public Task<string> GetValue(TimelineResolverModel resolverCommand);
}

public class CurrentUserRoleResolver(IClaimsProvider _claimsProvider) : IVariableResolver
{
    public async Task<string> GetValue(TimelineResolverModel resolverCommand) =>
        _claimsProvider.GetUserRoles().First();
}

public class CurrentUserNameResolver(IClaimsProvider _claimsProvider) : IVariableResolver
{
		public async Task<string> GetValue(TimelineResolverModel resolverCommand) =>
				_claimsProvider.GetUserName();
}

public class UploadedFileResolver : IVariableResolver
{
    public Task<string> GetValue(TimelineResolverModel resolverCommand)
    {
        return Task.FromResult(resolverCommand.AssetType==null ? string.Empty:
            $"{ resolverCommand.AssetType.ToDescription() + 
                (resolverCommand.AssetNumber==0 ? string.Empty:
                " "+resolverCommand.AssetNumber)
               }");
    }
}

public class ArticleNewStageResolver : IVariableResolver
{
    public Task<string> GetValue(TimelineResolverModel resolverCommand) =>
				Task.FromResult(resolverCommand.NewStage.ToString());
}

public class ArticleCurrentStageResolver : IVariableResolver
{
		public Task<string> GetValue(TimelineResolverModel resolverCommand) =>
				Task.FromResult(resolverCommand.CurrentStage.ToString());
}


//public class UserNameFileResolver(PersonRepository _personRepository) : IVariableResolver
//{
//    public async Task<string> GetValue(TimelineResolverModel resolverCommand)
//    {
//        var user = await _personRepository.GetByUserId(resolverCommand.Action.CreatedById);
//				return user.FullName;
//    }
//}

public class MessageResolver : IVariableResolver
{
    public async Task<string> GetValue(TimelineResolverModel resolverCommand)=> 
        resolverCommand.Action.Comment;
}