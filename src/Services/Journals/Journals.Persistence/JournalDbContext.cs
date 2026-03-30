namespace Journals.Persistence;

public class JournalDbContext
{
    private readonly RedisConnectionProvider _provider;

    public JournalDbContext(RedisConnectionProvider provider) =>
        _provider = provider;

    public IRedisCollection<Journal> Journals => _provider.RedisCollection<Journal>();
    //public IRedisCollection<Section> Sections => _provider.RedisCollection<Section>();
    public IRedisCollection<Editor> Editors => _provider.RedisCollection<Editor>();

    public RedisConnectionProvider Provider => _provider;
}
