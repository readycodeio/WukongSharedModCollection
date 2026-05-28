# SDK Report

# Changelog

[Feature Request](#feature-request)
- [Assety](#assety)
- [Debugger](#debugger)

# Spis Treści
1. [Instalacja](#1-instalacja)
<br> 1.1. [Klonowanie repozytorium](#11-klonowanie-repozytorium-mod-template)
<br> 1.2. [Uruchomienie custom server pierwszy raz](#12-uruchomienie-custom-server-pierwszy-raz)
2. [Tworzenie Moda](#2-tworzenie-moda)
<br> 2.1. [Niestniejące pole Version](#21-nieistniejące-pole-version)
<br> 2.2. [Szukanie pól i metod](#22-szukanie-pól-i-metod)
3. [RPC](#3-rpc-relaymode)
<br> 3.1. [Peers i EntityOwner](#31-relaymodepeers-relaymodeentityowner)
4. [Konsola](#4-konsola)
<br> 4.1. [Walidacja rejestracji](#41-brak-walidacji-przy-rejestracji)
<br> 4.2. [Statyczność availableFirstParams](#42-statyczność-availablefirstparams)
<br> 4.3. [Zgadywanie komend](#43-zgadywanie-komend-ux)
<br> 4.4. [Brak scroll](#44-brak-opcji-scroll-ux)
<br> 4.5. [Konsola traci focus](#45-konsola-traci-focus-ux)
5. [Sync](#5-sync)
<br> 5.1 [Pozycja](#51-pozycja-localmaincharacter)
<br> 5.2 [Manipulacja pozycją](#52-setlocationrotation-i-teleport)
<br> 5.3 [HP](#53-hp)
<br> 5.4 [Pawn](#54-localmaincharactervaluepawn)
<br> 5.5 [Transformacja](#55-transformacje)
<br> 5.6 [Inventory](#56-ekwipunek)


# 1. Instalacja
## 1.1. Klonowanie repozytorium mod-template
* Element „ExampleRpc” nie implementuje odziedziczonej abstrakcyjnej składowej „RpcClassBase.DeInitRpc()”
* Element „ExampleRpc” nie implementuje odziedziczonej abstrakcyjnej składowej „RpcClassBase.EventsCount.get”
* Element „ExampleRpc” nie implementuje odziedziczonej abstrakcyjnej składowej „RpcClassBase.InitRpc()”
* Element „ExampleRpc” nie zawiera definicji „SendExampleEvent” i nie odnaleziono dostępnej metody rozszerzenia „SendExampleEvent”, która przyjmuje pierwszy argument typu „ExampleRpc” (czy nie brakuje dyrektywy using lub odwołania do zestawu?).
* nie można rozpoznać wersji zestawu .NET SDK określonej w pliku global.json w lokalizacji `...mod-template\global.json`.

Poza numerem wersji .NET w global.json, brak jakiejkolwiek wzmianki o wymaganiach systemowych.
<br> Warto rozważyć sporządzenie sekcji w README o wymaganiu VS 2026 + .NET 10.

## 1.2. Uruchomienie custom server pierwszy raz
Pojawia się problem przy dołączaniu do custom server, jeżeli wcześniej nie dołączyliśmy do COOP lub PVP.
<br>Gra się uruchamia ale na ekranie głównym widnieje komunikat `"WukongMP.pak is missing"`
<br>Utworzenie osobiście pokoju COOP lub PVP i dołączenie naprawia ten błąd.
<br> 

# 2. Tworzenie moda

## 2.1. Nieistniejące pole "Version"
Dokumentacja dla <a href="https://docs.ready.mp/wukong-mp/docs/Development/Examples/swarm-mode">swarm-mode</a> twierdzi, że trzeba nadpisać pola:
```csharp
public class Mod : ModBase
{
    public override string Name => "Swarm";
    public override string Version => "1.0.0";
    ...
}
```
W obecnej wersji SDK pole Version nie istnieje.
```csharp
public class Mod : ModBase
{
    public override string Name => "PrivateComs";
    public override string Version => "1.0.0";
}
```
``` 
„Mod.Version”: nie znaleziono odpowiedniej metody do przesłonięcia
```
Dodatkowo pole Mod.Name nie jest do odczytu. Nie można go użyć np. do:
```csharp
Logging.LogWarning($"[{WukongApi.Sync.Mod.Name}] Custom log");
LUB
string x = Mod.Name;
```

## 2.2. Szukanie pól i metod
W obecnej wersji dokumentacji brakuje swego rodzaju ściągawki z najbardziej przydatnymi rzeczami.
Podstawowe rzeczy typu `WukongApi.Sync.LocalPlayerId` powinny być gdzieś na wierzchu. 
<br> Może dodatkowa zakładka pod <a href="[docs.ready.mp](https://docs.ready.mp/wukong-mp/category/mod-examples)">Mod examples</a>.
<br> Oczywiście da się to wszystko znaleźć samemu metodą prób i błędów za pomocą InteliSense, <br>wpisując okrojoną nazwę czego potrzebujemy i sprawdzać czy VS wyświetla to co nas interesuje.

# 3. RPC RelayMode
## 3.1. RelayMode.Peers, RelayMode.EntityOwner

Podczas tworzenia RPC zauważyłem, że InteliSense podpowiada dwa kolejne tryby komunikacji. Nie są one nigdzie wspomniane. Posiadają one wbudowane opisy, tak jakby były gotowe do użycia.<br> Jednakże, nie udało mi się ich poprawnie użyć. Ma to sens, ponieważ dokumentacja wspomina tylko cztery pierwsze.
<br><br>*Kod pozyskany przy użyciu klawisza F12*
```csharp
public enum RelayMode : byte
{
    //...
    AreaOfInterestOthers,
    //...
    AreaOfInterestAll,
    //...
    GlobalOthers,
    //...
    GlobalAll,
    //
    // Podsumowanie:
    //     Sends the message to the owner of an entity, possibly back to the sender.
    //
    // Uwagi:
    //     Not part of stable API.
    EntityOwner,
    //
    // Podsumowanie:
    //     Sends the message to a specific list of players.
    Peers
}
```

# 4. Konsola
## 4.1. Brak walidacji przy rejestracji
`WukongApi.Console.AddCommand()` nie waliduje nazwy. Możemy użyć dowolnego stringu. Próba wywołania takiej komendy to już inna sprawa.
Parser nie pozwoli wywołać żadnej komendy, gdy we wprowadzonym ciągu występują znaki specjalne lub cyfry niepoprzedzone literami.

### Przykłady:
```csharp
    WukongApi.Console.AddCommand("test123",ConsoleCommand.Create(() => { WukongApi.Chat.ShowLocalMessage("Test message with digits", FLinearColor.White); }));
```
OK `Test message with digits` 
```csharp
    WukongApi.Console.AddCommand("123",ConsoleCommand.Create(() => { WukongApi.Chat.ShowLocalMessage("Test message only digits", FLinearColor.White); }));
```
> Invalid command format: 123
```csharp
    WukongApi.Console.AddCommand("test.test",ConsoleCommand.Create(() => { WukongApi.Chat.ShowLocalMessage("Test message with dots", FLinearColor.White); }));
```
> Invalid command format: test.test
```csharp
    WukongApi.Console.AddCommand("test!@#",ConsoleCommand.Create(() => { WukongApi.Chat.ShowLocalMessage("Test message with special characters", FLinearColor.White); }));
```
> InvalidCommand format: test!@#
```csharp
    WukongApi.Console.AddCommand("test ",ConsoleCommand.Create(() => { WukongApi.Chat.ShowLocalMessage("Test message with space", FLinearColor.White); }));
```
> Unrecognized command: test <span>***<- tu jest spacja***</span>

## 4.2. Statyczność AvailableFirstParams
Opcja bardzo fajna. Dokumentacja twierdzi, że to dla **predefiniowanej** listy argumentów. W większości przypadów tak może być, jednak w moim potrzbowałem dynamicznie generowanych. Konkretnie listy nazw wszystkich graczy. Próbowałem kilka sposobów takich jak aktualizacja przekazanego obiektu, ponowna próba rejestracji z nową listą. Ostatecznie oczekiwany wynik przyniósł generator, co wbrew testom, nie powinno działać. Wykomentowana linijka zawsze zwracała te samą liczbę sekund. 
```csharp
    private static IEnumerable<string> GetLazyPlayerNames()
    {
        foreach (var player in WukongApi.Sync.AllPlayers.ToList())
        {
            WukongApi.Sync.TryGetPlayerInfoById(player, out string? playerName, out _);
            if (playerName is not null)
            {
                //yield return $"test {DateTime.Now.Second}";
                yield return playerName;
            }
        }
    }

    WukongApi.Console.AddCommand($"{CommandPrefix}message",ConsoleCommand.Create(CommandSendPrivateMessage), GetLazyPlayerNames());
```
Ten kod pozwala na wprowadzenie dynamicznej listy argumentów dla wskazanej komendy.
A przynajmniej tak wynika z testów z Jan Wąsowski. 
### <span style="color:orange"> Czasem działa czasem nie, klasyk.
Wymaga więcej testów.

## 4.3. Zgadywanie komend (UX)
Jako twórca oczywiście posiadam wiedzę o dostępnych komendach w moim modzie. Sprawa wygląda nieco inaczej, jeżeli chodzi o osoby z niego korzystające. W momencie kiedy zawiedzie dokumentacja moda, może warto było by rozważyć wprowadzenie wbudowanej komendy konsoli `help`, która wylistuje wszystkie zarejestrowane. W obecnej formie, trzeba zgadywać i odnosić się do wyniku z autouzupełnienia, które dopasowywuje na bieżąco.

## 4.4. Brak opcji scroll (UX)
Ani chat ani konsola nie wspierają scrollowania po historii wiadomości lub komend. Przy większej ilości pluginów, może to prowadzić do floodowania. Przykładowo gdy było by wiele custom systemów wypisujących na czat swój progress i.e (Swarm - "pozostało x wrogów").

## 4.5. Konsola traci focus (UX)
Po wciśnięciu klawisza Enter konsola wykonuje polecenie i traci focus.
Według mnie powinna tracić focus tylko gdy jest schowana.

# 5. Sync
## 5.1. Pozycja LocalMainCharacter
Bezpośrednie nadpisywanie pozycji nic nie daje. Prawdopodobnie Silnik fizyki trzyma prawdziwą wartość. Rozważyć zmianę na publiczny getter i private setter. Teoretycznie to wartość zmienia się na jedną klatkę, ale od razu wraca na swoje miejsce.

## 5.2. SetLocationRotation i Teleport
Nie działa. Nic nie robi. Jedyny sposób na zmianę pozycji to poprzez Teleport, które wydaje się nadawać nieskończone momentum, ponieważ blokuje się na colliderach.
Można wykonywać na innych graczach.
Przekazywane wartości muszą być duże aby mieć zauważalny wpływ. Zwykły skok to jakieś 300-400 jednostek. 

```csharp
public static void moveTest()
{
    var character = WukongApi.Sync.LocalMainCharacter.Value;
    var newLocation = new(character.Location.X, character.Location.Y, character.Location.Z + 2000);

    //Zero efektu
    character.SetLocationRotation(newLocation, character.Rotation);
}

public static void teleportOther()
{
    var characterNullable = WukongApi.Sync.AllMainCharacters
        .Cast<ReadyMainCharacter?>()
        .FirstOrDefault(c => c.Value.PlayerId != Mod.PlayerId);

    if (!characterNullable.HasValue) return;

    var otherCharacter = characterNullable.Value;
    var newLocation = new(otherCharacter.Location.X, otherCharacter.Location.Y, otherCharacter.Location.Z + 2000);

    //To działa
    //Wszystko jest synchronizowane
    otherCharacter.Teleport(newLocation, otherCharacter.Rotation);
}
```
## 5.3. HP
Bezpośredni dostęp do HP. Nadpisywanie LocalMainCharacter.Value.Hp skutecznie zmienia i synchronizuje je z innymi graczami. Nie można wykonywać na innych graczach.
<br>Wyrzuca: `System.InvalidOperationException: Operation is not valid due to the current state of the object.`<br> 
w `WukongMp.Sdk.Entities.ReadyCharacterExtensions.set_Hp[TSelf] (TSelf obj, System.Single value)`
<br>Oczywiście ma to sens, nie powinniśmy być w stanie manipulować innymi graczami.
```csharp
    public static void testOther()
    {
        try
        {
            //LocalPlayer
            var character = WukongApi.Sync.LocalMainCharacter.Value;
            character.Hp -= 50;

            //Inny gracz
            var characterNullable = WukongApi.Sync.AllMainCharacters
                .Cast<ReadyMainCharacter?>()
                .FirstOrDefault(c => c.Value.PlayerId != Mod.PlayerId);

            if (!characterNullable.HasValue)
            {
                WukongApi.Chat.ShowLocalMessage("No other characters found", FLinearColor.Red);
                return;
            }

            var otherCharacter = characterNullable.Value;
            otherCharacter.Hp -= 50;

            WukongApi.Chat.ShowLocalMessage($"{otherCharacter.Hp} / {otherCharacter.HpMaxBase}", FLinearColor.Red);
        }
        catch (Exception ex)
        {
            WukongApi.Chat.ShowLocalMessage($"crash testOther(): {ex.Message}");
        }
    }
```
## 5.4. LocalMainCharacter.Value.Pawn
Brak możliwości skorzystania. Brakuje odwołania do zestawu `BtlSvr.Main`

## 5.5. Transformacje
W sumie spoko była by możliwość odpalenia transformacji z poziomu kodu.
Oczywiście można by to zrobić przy użyciu HarmonyPatch i refleksji.

## 5.6. Ekwipunek
Dostęp do ekwipunku też byłby git. Podmienianie trzymanej broni na coś innego po każdym killu w PvP. Coś w stylu Gun Game z Couter Strike i Call of Duty.

## 5.7. Spawnowanie przeciwników [UX]
Jedyna uwaga to może rozważyć podzielenie enum na bossy i zwykłych przeciwników. Obecnie jest to jeden duży enum TamerKinds.<nazwa>.
<br> Z drugiej strony, modder raczej wie co chce zespawnować, i wie co jest a co nie jest bossem.
```csharp
    WukongApi.Sync.SpawnEnemy(TamerKinds.Bosses.<boss>, location);
    WukongApi.Sync.SpawnEnemy(TamerKinds.<zwykły>, location);
```

# Debugger
`Logging.LogDebug` nigdzie nie jest wypisywane. Testowane w konfiguracji Debug i Release. Także z włączonym debuggerem.
`LogError, LogWarning, LogInformation` pojawiają sie w pliku `AppData\Roaming\ReadyM.Launcher\WukongMP\wukong-mp-logs.log.json`

# Feature Request
Nieosiągalne bez Harmony lub niepubliczne
## Assety
- Wyeksponowanie funkcji w `WukongApi` do manipulacji aktorami:
    - Spawnowanie aktorów zarejestrowanych z pliku .pak
    - Obsługa aktora np.: ustawianie flag (EnableAi, EnableCollisionMask, EnableInteractionMask, Teleport)
    - Rejestrowanie nowych map i ich obsługa.
Uważam że bardzo zwiększyło by to możliwości w tworzeniu modów. Wyłączając AI aktorowi można by zrobić Hall of Fame, eksponujące wszystkich dotychczas pokonanych wrogów. Stawianie obiektów umożliwiło by robienie trybów parkour czy stawianie przeszkód w świecie aby utrudnić rozgrywkę. Customowy skill "Box" stawiający dosłownie pudło przed graczem aby zablokować jakiś atak dystansowy. 
## World
- Obsługa świata typu `WukongApi.World.SetArea(AreaId)` dla zmiany mapy.
Obecnie jesteśmy ograniczeni do liniowego przechodzenia mapy, wczytywania konkretnego save lub bycia uwięzionym na jednej w trybie pvp.
## Mode
- Trzeci tryb mieszany zamiast Coop lub Pvp. Były by one odpalane na żądanie np. `duel "playerName"`
Rozumiem jeżeli to nie jest możliwe ze względu na architekturę.

# Crash

## Losowy crash podczas nic nie robienia na serwerze. b1.BUS_Talent
`{"TimeGenerated":"2026-05-26T13:10:35.8631476Z","Level":"Critical","MessageTemplate":"Exception: {Message} | Thread: {Thread} | Stack trace: {Trace} | Context: [null] [thread __ThreadId at __Location]","Properties":{"Message":"An item with the same key has already been added. Key: 100102","Thread":1,"Trace":"System.ArgumentException: An item with the same key has already been added. Key: 100102\r\n  at System.Collections.Generic.Dictionary\u00602[TKey,TValue].TryInsert (TKey key, TValue value, System.Collections.Generic.InsertionBehavior behavior) [0x000dd] in \u003Cba770f7db8ff45699e1955c36f94a06a\u003E:0 \r\n  at System.Collections.Generic.Dictionary\u00602[TKey,TValue].Add (TKey key, TValue value) [0x00000] in \u003Cba770f7db8ff45699e1955c36f94a06a\u003E:0 \r\n  at b1.BUS_TalentComp.OnActivateTalent (System.Int32 TalentID, System.Int32 ChangeLevel) [0x003e0] in \u003C65a46ed76f8540f5ad32079a507252b6\u003E:0 \r\n  at b1.BUS_TalentComp.OnTickWithGroup_PreAnim (System.Single DeltaTime) [0x00041] in \u003C65a46ed76f8540f5ad32079a507252b6\u003E:0 \r\n  at b1.BUS_TalentComp.OnTickWithGroup (System.Single DeltaTime, System.Int32 TickGroup) [0x00005] in \u003C65a46ed76f8540f5ad32079a507252b6\u003E:0 \r\n  at b1.ECS.EntityArchetype.TickAllComponentImpl (System.Single DeltaTime, System.Int32 TickGroup, System.Int32 ThreadIdx, System.Int32 ThreadCount) [0x00079] in \u003C16cc10e724f84c7f9567ce775606bafd\u003E:0 \r\n  at b1.ECS.EntityArchetype.TickAllComponentWithGroup (System.Single DeltaTime, System.Int32 TickGroup, System.Int32 ThreadIdx, System.Int32 ThreadCount) [0x00000] in \u003C16cc10e724f84c7f9567ce775606bafd\u003E:0 \r\n  at b1.ECS.EntityManager.TickAllComponentsWithGroup (System.Single DeltaTime, System.Int32 TickGroup, System.Int32 ThreadIdx, System.Int32 ThreadCount) [0x00015] in \u003C16cc10e724f84c7f9567ce775606bafd\u003E:0 \r\n  at b1.ECS.EntityManager.TickAllComponentsWithGroup (System.Single DeltaTime, System.Int32 TickGroup) [0x00000] in \u003C16cc10e724f84c7f9567ce775606bafd\u003E:0 \r\n  at b1.BGW_ECSWorld.OnTickWithGroup (System.Single DeltaTime, System.Int32 TickGroup) [0x000d9] in \u003C65a46ed76f8540f5ad32079a507252b6\u003E:0 \r\n  at b1.BGWGameInstanceCS.TickAllGameInstComp (System.Single DeltaSeconds, System.Int32 BGWTickGroup) [0x000c6] in \u003C65a46ed76f8540f5ad32079a507252b6\u003E:0 \r\n  at b1.BGWGameInstanceCS.ReceiveTick_Implementation (System.Single DeltaSeconds, System.Int32 TickGroup) [0x000a7] in \u003C65a46ed76f8540f5ad32079a507252b6\u003E:0 \r\n  at b1.BGWGameInstanceCS.ReceiveTick__Invoker (System.IntPtr buffer, System.IntPtr obj) [0x00029] in \u003C65a46ed76f8540f5ad32079a507252b6\u003E:0 \r\n  at UnrealEngine.Runtime.ManagedUnrealTypes\u002BManagedClass.HandleInvokeFunctionFromNative (System.IntPtr obj, UnrealEngine.Runtime.FFrame* stack, System.IntPtr result, UnrealEngine.Runtime.UFunction\u002BFuncInvokerManaged managedFunctionInvoker) [0x0000e] in \u003C70421ab086dd4bd4afd66c909e8b51a7\u003E:0 \r\n  at UnrealEngine.Runtime.ManagedUnrealTypes\u002BManagedClass.InvokeFunctionImpl (System.IntPtr obj, System.IntPtr stackPtr, System.IntPtr result) [0x0003e] in \u003C70421ab086dd4bd4afd66c909e8b51a7\u003E:0 \r\n  at UnrealEngine.Runtime.ManagedUnrealTypes\u002BManagedClass.InvokeFunction (System.IntPtr obj, System.IntPtr stackPtr, System.IntPtr result) [0x00000] in \u003C70421ab086dd4bd4afd66c909e8b51a7\u003E:0 ","__ThreadId":1,"__Location":"WukongMp.Api.Patches.ExceptionPatches.Postfix"},"Session":"4667e385-1556-4df9-a42c-59d2dd51fb5d"}
`
