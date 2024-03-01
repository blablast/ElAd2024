using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Devices;
using HtmlAgilityPack;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Windows.Devices.AllJoyn;

namespace ElAd2024.Devices;

public partial class RobotDevice : ObservableRecipient, IRobotDevice
{
    #region Constructors

    public async Task InitializeAsync()
    {
        Debug.WriteLine($"RobotService initialized. Connecting status: {IsConnected}.");
        await Task.CompletedTask;
    }

    #endregion

    #region Fields

    private string connectionResult = string.Empty;
    private const int PingAttempts = 10;
    private const int PingTimeout = 250;

    #endregion

    #region Properties

    //public async Task<bool> IsConnectedAsync()
    //{
    //    IsConnected = await PingAsync();
    //    if (!IsConnected) await ConnectAsync();
    //}

    [ObservableProperty] private bool isConnected;
    [ObservableProperty] private bool isSimulated = false;
    public string IpAddress { get; set; } = string.Empty;
    public List<string> RobotVisions { get; set; } = [];
    public string RobotVisionUrl() => $"http://{IpAddress}/frh/vision/vrfrmn.stm";
    #endregion

    #region Public Methods

    public void GetRobotVisions() => RobotVisions = FanucData<string>.GetVisions(IpAddress);

    public async Task ConnectAsync(object? parameters = null)
    {
        if(parameters is not string ipAddress)
        {
            throw new ArgumentNullException(nameof(parameters));
        }
        IpAddress = ipAddress;

        IsConnected = await PingAsync();
        AppNotificationManager.Default.Show(
            new AppNotificationBuilder()
                .AddText(IsConnected ? "Robot Online!" : "Robot offline!")
                .AddText(connectionResult)
                .BuildNotification()
        );
        if (IsConnected)
        {
            GetRobotVisions();
        }
    }

    public Task DisconnectAsync() => throw new NotImplementedException();

    public async Task<bool> SetRegisterAsync(int index, bool value)
    {
        return await SetValueAsync($"http://{IpAddress}:3080/COMET/rpc?func=IOVALSET&type=35&index={index}&value={(value ? 1 : 0)}");
    }

    //http://{IpAddress}:3080/COMET/rpc?func=IOVALRD&type=35&index=7
    public async Task<bool> SetRegisterAsync(int index, int value)
    {
        Debug.WriteLine($"INT: {value}, http://{IpAddress}/karel/ComSet?sValue={value}&sIndx={index}&sRealFlag=0&sFc=2");
        return await SetValueAsync($"http://{IpAddress}/karel/ComSet?sValue={value}&sIndx={index}&sRealFlag=0&sFc=2");
    }

    public async Task<bool> SetRegisterAsync(int index, double value)
    {
        Debug.WriteLine($"DOUBLE: {value}, http://{IpAddress}/karel/ComSet?sValue={value.ToString("F6", CultureInfo.InvariantCulture)}&sIndx={index}&sRealFlag=1&sFc=2");
        return await SetValueAsync($"http://{IpAddress}/karel/ComSet?sValue={value.ToString("F6", CultureInfo.InvariantCulture)}&sIndx={index}&sRealFlag=1&sFc=2");
    }

    public async Task<bool> SetRegisterAsync(int index, string value)
        => await SetValueAsync($"http://{IpAddress}/karel/ComSet?sValue={value}&sIndx={index}&sFc=15");

    public async Task<bool> GetFlagRegisterAsync(int index)
    {
        IsIndexInRange(index, 1, 200);
        var response = await GetValueAsync<ResponseRpc>($"http://{IpAddress}:3080/COMET/rpc?func=IOVALRD&type=35&index={index}");
        var value = response?.Fanuc.RPC[0].Value;
        return response is not null && value == "1";
    }

    public async Task<(int?, double?)> GetNumericRegisterAsync(int index)
    {
        IsIndexInRange(index, 1, 200);
        var response = await GetValueAsync<ResponseRpc>($"http://{IpAddress}:3080/COMET/rpc?func=REGVALRD&index={index}");
        var value = response?.Fanuc.RPC[0].Value;

        if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var intValue))
        {
            return (intValue, null);
        }

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dblValue))
        {
            return (null, dblValue);
        }

        return (null, null);
    }

    // http://192.168.1.100:3080/COMET/rpc?func=REGVALRD&index=2
    public async Task<string> GetStringRegisterAsync(int index)
    {
        IsIndexInRange(index, 1, 25);
        await Task.CompletedTask;
        return (await FanucData<string>.GetRegistersAsync("STRREG.VA", IpAddress))[index - 1];
    }

    public void ChangeOverride(int value)
        => SetValueAsync($"http://{IpAddress}:3080/COMET/rpc?func=CHGOVRD&ovrd_val={value}").GetAwaiter().GetResult();

    public PositionXyzWpr CurrentPosition
        => new PositionXyzWpr().Decode(
            GetValueAsync<ResponseRpc>($"http://{IpAddress}/COMET/rpc?func=TXML_CURPOS&pos_rep=1&pos_type=1&grp_num=1")
                .GetAwaiter().GetResult()?.Fanuc.RPC[0].Value) ?? new PositionXyzWpr();

    // http://ROBOTIP/KCL/set%20port%20DOUT[1]=1 SET DO0 to 0

    #endregion

    #region private Methods

    private static bool IsIndexInRange(int index, int min, int max) => index < min || index > max
        ? throw new ArgumentOutOfRangeException(nameof(index))
        : true;

    private static readonly JsonSerializerOptions GetValueJsonSerializerOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    private static async Task<Response<T>?> GetValueAsync<T>(string url)
    {
        try
        {
            var response = await new HttpClient().GetAsync(url).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var deserialized = JsonSerializer.Deserialize<Response<T>>(responseBody, GetValueJsonSerializerOptions);
                return deserialized;
            }
            else
            {
                return null;
            }
        }
        catch
        {
            return null;
        }
    }

    private static async Task<bool> SetValueAsync(string url)
    {
        try
        {
            return (await new HttpClient().GetAsync(url).ConfigureAwait(false)).IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> PingAsync()
    {
        var ping = new Ping();
        PingReply? reply;
        var counter = 0;
        do
        {
            try
            {
                reply = await ping.SendPingAsync($"{IpAddress}", PingTimeout);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

            var response = new StringBuilder();
            if (reply.Status == IPStatus.Success)
            {
                response.AppendLine($"Address: {reply.Address}");
                response.AppendLine($"RoundTrip time: {reply.RoundtripTime} ms");
                response.AppendLine($"Time to live: {reply.Options?.Ttl} ms");
                response.AppendLine($"Succesfull attempt: {counter + 1}");
            }
            else
            {
                response.AppendLine(reply.Status.ToString());
                response.AppendLine($"Attempts: {counter + 1}");
                await Task.Delay(PingTimeout);
            }

            connectionResult = response.ToString();
        } while (++counter < PingAttempts && reply.Status != IPStatus.Success);

        return reply.Status == IPStatus.Success;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

internal class FanucData<T>
{
    public static List<string> GetVisions(string ipAddress)
    {
        List<string> registers = [];
        try
        {
            var responseBody = new HttpClient().GetStringAsync($"http://{ipAddress}/MD/INDEX_VD.HTM").GetAwaiter()
                .GetResult();
            HtmlDocument htmlSnippet = new();
            htmlSnippet.LoadHtml(responseBody);
            var words = htmlSnippet.DocumentNode?.SelectNodes("//table")[2]?.InnerText.Split('\n') ?? [];
            for (var i = 0; i < words.Length; i++)
            {
                if (words[i].Contains("Vision VD file"))
                {
                    registers.Add(words[i].Replace("Vision VD file", string.Empty).Trim().Replace(".VD", string.Empty));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

        return registers;
    }

    public static async Task<List<T>> GetRegistersAsync(string fanucFileName, string ipAddress)
    {
        HttpClient client = new();
        List<T> registers = [];
        try
        {
            var responseBody = await client.GetStringAsync($"http://{ipAddress}/MD/{fanucFileName}");
            var words = FanucRegHtmlParse(responseBody).Split('\n');

            for (var i = 2; i < words.Length - 4; i++)
            {
                var text = words[i][(words[i].IndexOf('=') + 2)..words[i].IndexOf('\'')].TrimEnd();
                if (typeof(T) == typeof(double) && double.TryParse(text, out var value))
                {
                    registers.Add((T)Convert.ChangeType(value, typeof(T)));
                }
                else if (typeof(T) == typeof(string))
                {
                    registers.Add((T)Convert.ChangeType(text, typeof(T)));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

        return registers;
    }

    private static string FanucRegHtmlParse(string html)
    {
        HtmlDocument htmlSnippet = new();
        htmlSnippet.LoadHtml(html);
        try
        {
            var pre = htmlSnippet.DocumentNode?.SelectNodes("//body/pre")[0];
            return pre?.InnerText ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    #endregion
}

internal class Response<T>
{
    public ResponseFanuc<T> Fanuc { get; set; } = new();
}

internal class ResponseFanuc<T>
{
    public string Name { get; set; } = string.Empty;
    public string FastClock { get; set; } = string.Empty;
    public List<T> RPC { get; set; } = [];
}

internal class ResponseRpc
{
    public string Rpc { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

internal class ResponseRpcFlag : ResponseRpc
{
    public string Type { get; set; } = string.Empty;
    public string Index { get; set; } = string.Empty;
}

public partial class PositionXyzWpr
{
    public double? X
    {
        get; set;
    }
    public double? Y
    {
        get; set;
    }
    public double? Z
    {
        get; set;
    }
    public double? W
    {
        get; set;
    }
    public double? P
    {
        get; set;
    }
    public double? R
    {
        get; set;
    }

    public bool IsValid => X is not null && Y is not null && Z is not null &&
                           W is not null && P is not null && R is not null;

    public PositionXyzWpr? Decode(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        // Match the pattern in the value string
        var match = PostionCorrectFormatRegex().Match(value);

        if (match.Success)
        {
            // Using double.Parse
            X = ParseAndRound(match.Groups[1].Value);
            Y = ParseAndRound(match.Groups[2].Value);
            Z = ParseAndRound(match.Groups[3].Value);
            W = ParseAndRound(match.Groups[4].Value);
            P = ParseAndRound(match.Groups[5].Value);
            R = ParseAndRound(match.Groups[6].Value);
        }
        else
        {
            // Set null values
            X = null;
            Y = null;
            Z = null;
            W = null;
            P = null;
            R = null;
        }
        return match.Success ? this : null;
    }

    public void Deconstruct(out double? x, out double? y, out double? z, out double? w, out double? p, out double? r)
    {
        x = X;
        y = Y;
        z = Z;
        w = W;
        p = P;
        r = R;
    }

    private static double? ParseAndRound(string value)
        => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue)
            ? Math.Round(parsedValue, 2)
            : null;

    public override string ToString()
        => $"X: {X} Y: {Y} Z: {Z} W: {W} P: {P} R: {R}";
    [GeneratedRegex(@"X:\s*([\d.-]+)\s*Y:\s*([\d.-]+)\s*Z:\s*([\d.-]+)\s*W:\s*([\d.-]+)\s*P:\s*([\d.-]+)\s*R:\s*([\d.-]+)")]
    private static partial Regex PostionCorrectFormatRegex();
}