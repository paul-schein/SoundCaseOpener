using NSubstitute;
using OneOf.Types;
using SoundCaseOpener.Core.Services;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Repositories;
using SoundCaseOpener.Persistence.Util;
using Xunit;

namespace SoundCaseOpener.Test;

public class ItemTemplateTests
{
    private readonly IUnitOfWork _mockUnit;
    private readonly ILogger<ItemTemplateService> _mockLogger;
    private readonly ItemTemplateService _sut;

    public ItemTemplateTests()
    {
        _mockUnit = Substitute.For<IUnitOfWork>();
        _mockLogger = Substitute.For<ILogger<ItemTemplateService>>();
        _sut = new ItemTemplateService(_mockUnit, _mockLogger);
    }

    [Theory]
    [InlineData(1, 
                   "Test Item",
                   "Test Description", 
                   Rarity.Common)]
    public async Task DeleteItemTemplateAsync_Success(int id, 
                                                      string name, 
                                                      string description,
                                                      Rarity rarity)
    {
        var repoMock = Substitute.For<ITemplateRepository<ItemTemplate>>();
        var itemTemplate = new ItemTemplate
        {
            Id = id,
            Name = name,
            Description = description,
            Rarity = rarity,
            CaseTemplates = [],
            Items = []
        };
        repoMock.GetByIdAsync(id).Returns(itemTemplate);
        _mockUnit.ItemTemplateRepository.Returns(repoMock);

        var result = await _sut.DeleteItemTemplateAsync(id);

        Assert.IsType<Success>(result);
        repoMock.Received(1).Remove(itemTemplate);
        await _mockUnit.Received(1).SaveChangesAsync();
    }

    [Theory]
    [InlineData(2)]
    public async Task DeleteItemTemplateAsync_NotFound(int id)
    {
        var repoMock = Substitute.For<ITemplateRepository<ItemTemplate>>();
        repoMock.GetByIdAsync(id).Returns((ItemTemplate?)null);
        _mockUnit.ItemTemplateRepository.Returns(repoMock);

        var result = await _sut.DeleteItemTemplateAsync(id);

        Assert.IsType<NotFound>(result);
        repoMock.DidNotReceive().Remove(Arg.Any<ItemTemplate>());
        await _mockUnit.DidNotReceive().SaveChangesAsync();
    }
}