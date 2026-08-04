// Decompiled with JetBrains decompiler
// Type: SeedWorks.ICommand
// Assembly: SeedWorks, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DE7A8622-820A-4E6F-983B-46E4E681EE48
// Assembly location: C:\Users\LysakovskiyAS\.nuget\packages\edms1.seedworks\1.0.1.52\lib\netstandard2.0\SeedWorks.dll
// XML documentation location: C:\Users\LysakovskiyAS\.nuget\packages\edms1.seedworks\1.0.1.52\lib\netstandard2.0\SeedWorks.xml


#nullable disable
namespace EDMS1.CommandLog.Commands;

/// <summary>Интерфейс команды над агрегатом.</summary>
public interface ICommand
{
    /// <summary>Токен корреляции для сквозных процессов.</summary>
    Guid CorrelationToken { get; }
}
