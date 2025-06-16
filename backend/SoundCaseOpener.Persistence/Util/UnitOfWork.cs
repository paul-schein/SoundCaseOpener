using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Repositories;

namespace SoundCaseOpener.Persistence.Util;

public interface ITransactionProvider : IAsyncDisposable, IDisposable
{
    public ValueTask BeginTransactionAsync();
    public ValueTask CommitAsync();
    public ValueTask RollbackAsync();
}

public interface IUnitOfWork
{
    public IUserRepository UserRepository { get; }
    public ISoundFileRepository SoundFileRepository { get; }
    public ITemplateRepository<SoundTemplate> SoundTemplateRepository { get; }
    public ITemplateRepository<CaseTemplate> CaseTemplateRepository { get; }
    public ITemplateRepository<ItemTemplate> ItemTemplateRepository { get; }
    public ICaseItemRepository CaseItemRepository { get; }
    
    public Task SaveChangesAsync();
}

internal sealed class UnitOfWork(DatabaseContext context, ILogger<UnitOfWork> logger)
    : IUnitOfWork, ITransactionProvider
{
    private IDbContextTransaction? _transaction;

    public IUserRepository UserRepository { get; } = new UserRepository(context.Users);
    public ISoundFileRepository SoundFileRepository { get; } = new SoundFileRepository(context.SoundFiles);
    public ITemplateRepository<SoundTemplate> SoundTemplateRepository { get; } = 
        new TemplateRepository<SoundTemplate>(context.SoundTemplates);
    public ITemplateRepository<CaseTemplate> CaseTemplateRepository { get; } = 
        new TemplateRepository<CaseTemplate>(context.CaseTemplates);
    public ITemplateRepository<ItemTemplate> ItemTemplateRepository { get; } = 
        new TemplateRepository<ItemTemplate>(context.ItemTemplates);
    public ICaseItemRepository CaseItemRepository { get; } = new CaseItemRepository(context.CaseItems);
    
    public async ValueTask BeginTransactionAsync()
    {
        if (_transaction is not null)
        {
            throw new TransactionException("Transaction already started, unable to start another");
        }

        _transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Snapshot);
    }

    public async ValueTask CommitAsync()
    {
        if (_transaction is null)
        {
            throw new TransactionException("No transaction started, unable to commit");
        }

        await _transaction.CommitAsync();
        _transaction = null;
    }

    public async ValueTask RollbackAsync()
    {
        if (_transaction is null)
        {
            throw new TransactionException("No transaction started, unable to rollback");
        }

        await _transaction.RollbackAsync();
        _transaction = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is null)
        {
            return;
        }

        // Transaction was neither committed nor rolled back, rolling back now - silent, this is acceptable
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
    }

    public void Dispose()
    {
        if (_transaction is null)
        {
            return;
        }

        logger
            .LogWarning($"Transaction was not disposed in {nameof(DisposeAsync)} and will now be rolled back and disposed in {nameof(Dispose)}");
        _transaction.Rollback();
        _transaction.Dispose();
    }

    public Task SaveChangesAsync() => context.SaveChangesAsync();

    private sealed class TransactionException(string message) : Exception(message);
}
