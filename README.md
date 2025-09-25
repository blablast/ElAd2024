# ElAd2024 - System Sterowania Robotów Przemysłowych i Testowania

## 🤖 Opis Projektu

ElAd2024 to zaawansowany system sterowania robotami przemysłowymi i zarządzania automatycznymi procedurami testowymi, opracowany w technologii WinUI 3. System umożliwia kompleksową kontrolę robotów FANUC, zarządzanie algorytmami testowymi, komunikację z urządzeniami poprzez porty szeregowe oraz pełne zarządzanie przepływem pracy testowej.

## ✨ Główne Funkcjonalności

### 🔧 Sterowanie Robotem
- **Bezpośrednia komunikacja** z robotami przemysłowymi FANUC poprzez TCP/IP
- **Kontrola pozycjonowania** - precyzyjne sterowanie pozycjami robota
- **Monitoring stanu** w czasie rzeczywistym
- **Zarządzanie trajektoriami** - definiowanie ścieżek ruchu
- **Obsługa komend robotycznych** - wykonywanie złożonych sekwencji

### 🧪 Zarządzanie Testami
- **Tworzenie algorytmów testowych** - konfigurowalne procedury
- **Wykonywanie automatycznych testów** z pełną kontrolą parametrów
- **Zarządzanie partiami testowymi** - organizacja testów w grupy
- **Historia testów** - kompletna dokumentacja wykonanych procedur
- **Analiza wyników** - szczegółowe raporty i statystyki

### 📡 Komunikacja Szeregowa
- **Interfejs z urządzeniami Arduino** (konwertery UART, emulatory PSOC)
- **Automatyczne wykrywanie portów** szeregowych
- **Konfiguracja parametrów komunikacji** (prędkość, bity danych, kontrola parzystości)
- **Monitoring ruchu danych** w czasie rzeczywistym
- **Obsługa wielu urządzeń** jednocześnie

### 💾 Integracja z Bazą Danych
- **Baza danych SQLite** do przechowywania wyników i konfiguracji
- **Automatyczne tworzenie kopii zapasowych**
- **Eksport danych** do różnych formatów
- **Indeksowanie i wyszukiwanie** wyników testów
- **Zarządzanie metadanymi** urządzeń i algorytmów

### 📊 Monitorowanie i Raportowanie
- **Widok w czasie rzeczywistym** stanu wszystkich urządzeń
- **Logi systemowe** z różnymi poziomami szczegółowości
- **Powiadomienia** o ważnych zdarzeniach
- **Generowanie raportów** w formacie PDF/Excel
- **Wykresy i wizualizacje** wyników testów

## 🏗️ Architektura Systemu

```
ElAd2024/
├── 📱 Aplikacja Główna (WinUI 3)
│   ├── Views/ - Interfejs użytkownika
│   ├── ViewModels/ - Logika prezentacji (MVVM)
│   └── Models/ - Modele danych
│
├── ⚙️ Zarządzanie Urządzeniami
│   ├── RobotDevice.cs - Komunikacja z robotami FANUC
│   ├── MediaDevice.cs - Obsługa urządzeń multimedialnych
│   ├── AllDevices.cs - Centralne zarządzanie urządzeniami
│   ├── Serial/ - Interfejsy urządzeń Arduino
│   │   └── BaseSerialDevice.cs - Bazowa klasa urządzeń szeregowych
│   └── Simulator/ - Symulacja urządzeń do testów
│
├── 🔧 Usługi (Services)
│   ├── DatabaseService.cs - Zarządzanie bazą danych SQLite
│   ├── ProceedTestService.cs - Silnik wykonywania testów
│   ├── SerialPortManagerService.cs - Zarządzanie komunikacją szeregową
│   └── LocalSettingsService.cs - Konfiguracja aplikacji
│
├── 📋 Modele Danych
│   ├── Database/ - Encje bazy danych
│   ├── TestParameters.cs - Parametry testów
│   └── SerialPortInfo.cs - Informacje o portach szeregowych
│
└── 🔌 Projekty Arduino
    ├── EA_UART_Converter/ - Konwerter mostka UART
    ├── EA_HumidityTemp/ - Czujnik wilgotności i temperatury
    └── PSOC_Emulator/ - Emulator sprzętowy PSOC
```

## 🎯 Główne Komponenty

### Robot Device (RobotDevice.cs)
- Zarządzanie połączeniem TCP/IP z robotem
- Parsowanie odpowiedzi HTML z interfejsu robota
- Kontrola wizji robotycznej
- Obsługa komend pozycjonowania
- Monitoring stanu połączenia

### Test Service (ProceedTestService.cs)
- Orkiestracja całego procesu testowego
- Zarządzanie fazami testów
- Integracja z urządzeniami sprzętowymi
- Zbieranie i analiza wyników
- Obsługa błędów i wyjątków

### Serial Manager (SerialPortManagerService.cs)
- Automatyczne skanowanie dostępnych portów
- Konfiguracja parametrów komunikacji
- Buforowanie danych
- Obsługa zdarzeń komunikacyjnych
- Zarządzanie wieloma połączeniami

## 💻 Wymagania Systemowe

### Minimalne Wymagania
- **System operacyjny**: Windows 10 version 19041 lub nowszy
- **Framework**: .NET 8.0 Runtime
- **Pamięć RAM**: 4 GB
- **Miejsce na dysku**: 500 MB
- **Rozdzielczość**: 1280x720

### Zalecane Wymagania
- **System operacyjny**: Windows 11
- **Framework**: .NET 8.0 SDK (do rozwoju)
- **Pamięć RAM**: 8 GB lub więcej
- **Miejsce na dysku**: 1 GB
- **Rozdzielczość**: 1920x1080 lub wyższa

### Wymagania Sprzętowe
- **Robot przemysłowy** kompatybilny z FANUC
- **Konwerter UART** oparty na Arduino
- **Sprzęt emulacji PSOC**
- **Interfejsy komunikacji szeregowej**
- **Połączenie sieciowe** Ethernet do komunikacji z robotem

## 🛠️ Instalacja i Konfiguracja

### Instalacja ze Źródeł
```bash
# 1. Sklonuj repozytorium
git clone <repository-url>
cd ElAd2024

# 2. Otwórz w Visual Studio 2022
start ElAd2024.sln

# 3. Przywróć pakiety NuGet
dotnet restore

# 4. Skompiluj rozwiązanie
dotnet build --configuration Release

# 5. Uruchom aplikację
dotnet run --project ElAd2024
```

### Konfiguracja Początkowa

#### 1. Ustawienia Robota
```json
{
  "RobotSettings": {
    "IpAddress": "192.168.1.100",
    "Port": 80,
    "TimeoutMs": 5000,
    "MaxRetries": 3
  }
}
```

#### 2. Konfiguracja Portów Szeregowych
- Uruchom aplikację i przejdź do **Ustawienia > Porty szeregowe**
- Wybierz odpowiednie porty dla urządzeń Arduino
- Skonfiguruj parametry komunikacji (domyślnie: 9600 baud, 8N1)

#### 3. Inicjalizacja Bazy Danych
Baza danych zostanie automatycznie utworzona przy pierwszym uruchomieniu w lokalizacji:
```
%LOCALAPPDATA%/ElAd2024/Database/
```

## 🚀 Użytkowanie

### 1. Uruchomienie Systemu
1. **Sprawdzenie połączeń** - Upewnij się, że robot i urządzenia Arduino są podłączone
2. **Uruchomienie aplikacji** - Otwórz ElAd2024.exe
3. **Weryfikacja statusu** - Sprawdź status wszystkich urządzeń w głównym oknie

### 2. Konfiguracja Urządzeń
```csharp
// Przykład konfiguracji robota w kodzie
await robotDevice.ConnectAsync("192.168.1.100");
if (await robotDevice.IsConnectedAsync())
{
    // Robot gotowy do pracy
}
```

### 3. Tworzenie Algorytmu Testowego
1. Przejdź do **Zarządzanie > Algorytmy**
2. Kliknij **Nowy algorytm**
3. Skonfiguruj parametry:
   - Nazwa algorytmu
   - Opis procedury
   - Parametry testowe
   - Kryteria sukcesu/niepowodzenia

### 4. Wykonywanie Testów
1. Wybierz **Test > Nowy test**
2. Wybierz algorytm z listy
3. Ustaw parametry specyficzne dla testu
4. Kliknij **Rozpocznij test**
5. Monitoruj postęp w czasie rzeczywistym

### 5. Zarządzanie Partiami
```
Partia testowa może zawierać:
├── Wiele algorytmów testowych
├── Różne parametry dla każdego testu
├── Harmonogram wykonania
└── Kryteria zakończenia partii
```

## 🔧 Komponenty Arduino

### EA_UART_Converter
**Plik**: `EA_UART_Converter/EA_UART_Converter.ino`

Konwerter mostka komunikacyjnego między PC a urządzeniami testowymi:
```cpp
void setup() {
  Serial.begin(115200);   // Komunikacja z PC
  Serial1.begin(9600);    // Komunikacja z urządzeniem
}

void loop() {
  // Przekazywanie danych PC → Urządzenie
  if (Serial.available()) {
    char data = Serial.read();
    Serial1.write(data);
  }

  // Przekazywanie danych Urządzenie → PC
  if (Serial1.available()) {
    char data = Serial1.read();
    Serial.print("Otrzymano: ");
    Serial.println((int)data);
  }
}
```

### PSOC_Emulator
**Plik**: `PSOC_Emulator/PSOC_Emulator.ino`

Emulator sprzętowy do symulacji odpowiedzi urządzeń:
```cpp
struct ValueRange {
  int value;
  int minRange;
  int maxRange;
};

// Zarządzanie fazami testowymi
int currentPhase = 0;
ValueRange settings[50];

void managePhaseFour() {
  // Implementacja fazy 4 testowania
  // z kontrolą LED i wartości wysokiego napięcia
}
```

## 🛠️ Technologie i Biblioteki

### Framework Główny
- **.NET 8.0** - Platforma wykonawcza
- **WinUI 3** - Nowoczesny interfejs użytkownika Windows
- **Windows App SDK 1.4** - Rozszerzone API Windows

### Biblioteki UI
- **Telerik WinUI Controls 2.8.0** - Zaawansowane kontrolki UI
- **WinUIEx 2.3.3** - Rozszerzenia dla WinUI
- **Microsoft.Xaml.Behaviors** - Zachowania XAML

### Zarządzanie Danymi
- **Entity Framework Core 8.0** - ORM dla bazy danych
- **SQLite** - Lokalna baza danych
- **Newtonsoft.Json 13.0** - Serializacja JSON

### Wzorce Architektoniczne
- **MVVM Toolkit 8.2** - Implementacja wzorca MVVM
- **Dependency Injection** - Wstrzykiwanie zależności
- **Async/Await** - Programowanie asynchroniczne

### Komunikacja
- **System.IO.Ports** - Komunikacja szeregowa
- **TCP/IP** - Komunikacja sieciowa z robotem
- **HTML Agility Pack 1.11** - Parsowanie odpowiedzi HTML

## 🎮 Interfejs Użytkownika

### Główne Strony Aplikacji

#### MainPage.xaml
- **Panel sterowania** - Centralne miejsce kontroli systemu
- **Status urządzeń** - Monitoring stanu w czasie rzeczywistym
- **Szybkie akcje** - Najczęściej używane funkcje

#### TestResultsPage.xaml
- **Historia testów** - Przeglądanie wykonanych testów
- **Filtrowanie wyników** - Wyszukiwanie po różnych kryteriach
- **Eksport danych** - Generowanie raportów

#### SettingsPage.xaml
- **Konfiguracja urządzeń** - Ustawienia robotów i portów szeregowych
- **Parametry aplikacji** - Dostosowanie zachowania systemu
- **Zarządzanie bazą danych** - Narzędzia administracyjne

#### ManageAlgorithmsPage.xaml
- **Tworzenie algorytmów** - Kreator nowych procedur testowych
- **Edycja istniejących** - Modyfikacja parametrów algorytmów
- **Import/Export** - Wymiana algorytmów między systemami

### Kontrolki Użytkownika
```
UserControls/
├── Slider.xaml - Niestandardowe suwaki do kontroli wartości
├── DeviceStatus.xaml - Komponenty statusu urządzeń
└── TestProgress.xaml - Wskaźniki postępu testów
```

## 📊 Baza Danych

### Struktura Tabel
```sql
-- Tabela algorytmów testowych
CREATE TABLE Algorithms (
    Id INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    Parameters TEXT, -- JSON
    CreatedDate DATETIME,
    ModifiedDate DATETIME
);

-- Tabela wyników testów
CREATE TABLE TestResults (
    Id INTEGER PRIMARY KEY,
    AlgorithmId INTEGER,
    TestDate DATETIME,
    Result TEXT, -- JSON
    Status TEXT, -- Success/Failed/Aborted
    Duration INTEGER, -- w sekundach
    FOREIGN KEY (AlgorithmId) REFERENCES Algorithms(Id)
);

-- Tabela konfiguracji urządzeń
CREATE TABLE DeviceConfigurations (
    Id INTEGER PRIMARY KEY,
    DeviceType TEXT,
    Configuration TEXT, -- JSON
    IsActive BOOLEAN,
    LastModified DATETIME
);
```

### Operacje na Danych
```csharp
// Przykład zapisu wyniku testu
public async Task SaveTestResultAsync(TestResult result)
{
    using var context = new DatabaseContext();
    context.TestResults.Add(result);
    await context.SaveChangesAsync();
}

// Pobieranie historii testów
public async Task<List<TestResult>> GetTestHistoryAsync(DateTime from, DateTime to)
{
    using var context = new DatabaseContext();
    return await context.TestResults
        .Where(tr => tr.TestDate >= from && tr.TestDate <= to)
        .Include(tr => tr.Algorithm)
        .ToListAsync();
}
```

## 🔐 Bezpieczeństwo i Niezawodność

### Mechanizmy Bezpieczeństwa
- **Walidacja danych** wejściowych przed komunikacją z robotem
- **Timeout'y komunikacyjne** zapobiegające zawieszeniu systemu
- **Logowanie błędów** dla celów diagnostycznych
- **Kopie zapasowe** bazy danych wykonywane automatycznie

### Obsługa Błędów
```csharp
try
{
    await robotDevice.ExecuteCommandAsync(command);
}
catch (TimeoutException)
{
    // Obsługa przekroczenia czasu
    logger.LogWarning("Robot communication timeout");
}
catch (ConnectionException ex)
{
    // Obsługa błędów połączenia
    logger.LogError(ex, "Failed to connect to robot");
}
```

### Monitoring Systemu
- **Health checks** urządzeń w regularnych interwałach
- **Automatyczne restart** połączeń w przypadku błędów
- **Powiadomienia** o krytycznych zdarzeniach
- **Metryki wydajności** systemu

## 📈 Rozszerzenia i Rozwój

### Planowane Funkcjonalności
- 🔄 **Integracja z systemami ERP** - Wymiana danych z systemami zarządzania
- 📱 **Aplikacja mobilna** - Monitoring z urządzeń mobilnych
- 🤖 **AI/ML** - Inteligentna analiza wyników testów
- ☁️ **Synchronizacja w chmurze** - Backup i synchronizacja danych

### API i Rozszerzalność
```csharp
// Interfejs dla nowych typów urządzeń
public interface IDevice
{
    Task<bool> ConnectAsync();
    Task<bool> IsConnectedAsync();
    Task ExecuteCommandAsync(string command);
    event EventHandler<DeviceEventArgs> StatusChanged;
}

// Przykład implementacji nowego urządzenia
public class CustomDevice : IDevice
{
    public async Task<bool> ConnectAsync()
    {
        // Implementacja połączenia
        return await Task.FromResult(true);
    }
}
```

## 📚 Dokumentacja dla Programistów

### Wzorce Projektowe
- **MVVM (Model-View-ViewModel)** - Separacja logiki od interfejsu
- **Repository Pattern** - Abstrakcja dostępu do danych
- **Observer Pattern** - Powiadomienia o zmianach stanu
- **Factory Pattern** - Tworzenie instancji urządzeń

### Konwencje Kodowania
```csharp
// Nazewnictwo
public class TestResultService          // PascalCase dla klas
private readonly ILogger _logger;       // camelCase ze _ dla pól prywatnych
public async Task ProcessAsync()        // Async suffix dla metod asynchronicznych

// Obsługa wyjątków
public async Task<Result<T>> TryExecuteAsync<T>(Func<Task<T>> operation)
{
    try
    {
        var result = await operation();
        return Result<T>.Success(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Operation failed");
        return Result<T>.Failure(ex.Message);
    }
}
```

## 🔧 Debugowanie i Diagnostyka

### Logi Systemowe
```csharp
// Konfiguracja logowania
public void ConfigureLogging(ILoggingBuilder builder)
{
    builder.AddConsole()
           .AddFile("logs/elad-{Date}.txt")
           .SetMinimumLevel(LogLevel.Information);
}

// Użycie w kodzie
_logger.LogInformation("Starting test algorithm: {AlgorithmName}", algorithm.Name);
_logger.LogWarning("Device connection unstable: {DeviceId}", deviceId);
_logger.LogError(ex, "Critical error in test execution");
```

### Narzędzia Diagnostyczne
- **Performance Counters** - Monitorowanie wydajności
- **Memory Profiling** - Analiza zużycia pamięci
- **Network Monitoring** - Śledzenie komunikacji sieciowej
- **Database Query Analysis** - Optymalizacja zapytań

## 🤝 Współpraca i Wsparcie

### Struktura Zespołu
- **Lead Developer** - Architektura systemu
- **Hardware Engineer** - Integracja z urządzeniami
- **Test Engineer** - Procedury testowe
- **UI/UX Designer** - Interface użytkownika

### Proces Rozwoju
1. **Planning** - Analiza wymagań i planowanie sprintów
2. **Development** - Implementacja funkcjonalności
3. **Testing** - Testy jednostkowe i integracyjne
4. **Code Review** - Przegląd kodu przez zespół
5. **Deployment** - Wdrożenie w środowisku produkcyjnym

### Zgłaszanie Błędów
```
Przy zgłaszaniu błędów należy podać:
├── Dokładny opis problemu
├── Kroki do odtworzenia
├── Oczekiwane zachowanie
├── Rzeczywiste zachowanie
├── Logi systemowe
└── Zrzuty ekranu (jeśli dotyczy)
```

## 📄 Licencja i Prawa Autorskie

**ElAd2024** to oprogramowanie przemysłowe o charakterze własnościowym, opracowane dla konkretnych potrzeb automatyzacji procesów produkcyjnych. Wszelkie prawa zastrzeżone.

### Warunki Użytkowania
- Oprogramowanie przeznaczone wyłącznie do użytku wewnętrznego
- Zakaz dystrybucji bez zgody autora
- Wsparcie techniczne dostępne na żądanie
- Regularne aktualizacje i poprawki bezpieczeństwa

---

**Ostatnia aktualizacja**: Marzec 2024
**Wersja dokumentacji**: 1.0
**Kontakt**: Opiekun projektu ElAd2024