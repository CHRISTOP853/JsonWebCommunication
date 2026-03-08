using Microsoft.Win32;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using JsonCore;
using JsonCore.Api;
using JsonCore.Services;
using JsonCore.Messaging;

namespace JsonGui
{
public partial class MainWindow : Window
{
    // private static readonly HttpClient _http = new HttpClient();
    private readonly TeamStatsService _teamService;
    private readonly JsonTextService _jsonService;
    
    private readonly ApiRequestQueue
    _queue = new ApiRequestQueue(); // 1 request at a time to avoid rate limits

    // In-memory JSON model (easy to modify)
     private JsonNode? _root;
    // private TreeViewItem? _selectedItem;

    //========================= Helper Records =========================

   

    //========================= Constructor =========================
    public MainWindow()
    {
        InitializeComponent();
        SetStatus("Ready.");
        _teamService = new TeamStatsService(new SnoozleApiClient());
        _jsonService = new JsonTextService();
    //     var api = new SnoozleApiClient();
    //     _service = new TeamStatsService(api);
    }

//=========================
        // Helper methods
        // =========================

        private void SetStatus(string msg)
        {
            if (txtStatus != null)
                txtStatus.Text = msg;
        }

        private void ShowError(string msg, Exception ex)
        {
            MessageBox.Show(
                $"{msg}\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            SetStatus(msg);
        }

        private void EnsureParsed()
        {
            if (_root == null)
                throw new 
                InvalidOperationException("No JSON has been parsed yet. Click 'Parse' after loading or pasting JSON.");

        }

        // private TreeViewItem BuildTreeItem(string label, JsonNode node)
        // {
        //     var item = new TreeViewItem
        //     {
        //         Header = label,
        //         Tag = node
        //     };

        //     if (node is JsonObject obj)
        //     {
        //         foreach (var kvp in obj)
        //         {
        //             if (kvp.Value is null)
        //                 item.Items.Add(new TreeViewItem { Header = $"{kvp.Key}: null", Tag = null });
        //             else
        //                 item.Items.Add(BuildTreeItem(kvp.Key, kvp.Value));
        //         }
        //     }
        //     else if (node is JsonArray arr)
        //     {
        //         for (int i = 0; i < arr.Count; i++)
        //         {
        //             var child = arr[i];
        //             if (child is null)
        //                 item.Items.Add(new TreeViewItem { Header = $"[{i}]: null", Tag = null });
        //             else
        //                 item.Items.Add(BuildTreeItem($"[{i}]", child));
        //         }
        //     }
        //     else
        //     {
        //         // Value node
        //         item.Header = $"{label}: {node.ToJsonString}";
        //     }

        //     return item;
        // }
    //      private JsonNode? GetSelectedNode()
    // {
    //     return _selectedItem?.Tag as JsonNode;
    // }


    // =========================
    // UI Event Handlers
    // =========================

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dlg = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                rbFile.IsChecked = true;
                txtSource.Text = dlg.FileName;
                SetStatus("File selected.");
            }
        }
        catch (Exception ex)
        {
            ShowError("Browse failed", ex);
        }
    }

    private async void Load_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (rbPaste.IsChecked == true)
            {
                SetStatus("Paste mode: JSON already in box.");
                return;
            }

            if (rbFile.IsChecked == true)
            {
                var path = txtSource.Text?.Trim();
                // if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                //     throw new FileNotFoundException("File path is missing or invalid.");

                txtJson.Text = await File.ReadAllTextAsync(path);
                SetStatus("Loaded JSON from file.");
                return;
            }

            if (rbUrl.IsChecked == true)
            {
                // var url = txtSource.Text?.Trim();
                // if (string.IsNullOrWhiteSpace(url))
                //     throw new ArgumentException("URL is missing.");

                txtJson.Text = await _jsonService.LoadFromUrlAsync(txtSource.Text.Trim());
                SetStatus("Loaded JSON from URL.");
                return;
            }

            // SetStatus("Select a source mode first.");
        }
        catch (Exception ex)
        {
            ShowError("Load failed", ex);
        }
    }
private void LoadTeam_Click(object sender, RoutedEventArgs e)
{
    _queue.Enqueue(async () =>
    {
        if (!int.TryParse(txtTeamNumber.Text.Trim(), out int teamNumber))
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show("Invalid team number.");
            });
            return;
        }

        var api = new SnoozleApiClient();
        var service = new TeamStatsService(api);

        var team = await service.GetTeamSeasonAsync(teamNumber);

        Dispatcher.Invoke(() =>
        {
            dataGrid.ItemsSource = team.Games;
            txtDataGridStatus.Text = $"Loaded {team.Games.Count} games.";
        });
    });
}

    private void Parse_Click(object sender, RoutedEventArgs e)
    {
        // MessageBox.Show("Parse clicked");
        try
        {
            // var json = txtJson.Text;
            // if (string.IsNullOrWhiteSpace(json))
            //     throw new ArgumentException("No JSON to parse.");

            _root = _jsonService.Parse(txtJson.Text);
            // if (_root is null) throw new Exception("Parsed JSON root is null.");

            // RenderTree(_root);
            // txtOutput.Text = "";
            // txtQueryResult.Text = "";
            // lblSelected.Text = "Selected: (none)";
            SetStatus("Parsed JSON successfully.");
        }
        catch (Exception ex)
        {
            ShowError("Parse failed", ex);
        }
    }

    private void PrettyPrint_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // EnsureParsed();

            // txtOutput.Text = _root!.ToJsonString(new JsonSerializerOptions
            // {
            //     WriteIndented = true
            // });
            txtJson.Text = _jsonService.PrettyPrint(txtJson.Text);

            SetStatus("Pretty printed JSON.");
        }
        catch (Exception ex)
        {
            ShowError("Pretty print failed", ex);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            EnsureParsed();

            var dlg = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FileName = "pretty.json"
            };

            if (dlg.ShowDialog() == true)
            {
                var pretty = _root!.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dlg.FileName, pretty);
                SetStatus("Saved pretty JSON to file.");
            }
        }
        catch (Exception ex)
        {
            ShowError("Save failed", ex);
        }
    }

//     private void RunQuery_Click(object sender, RoutedEventArgs e)
//     {
//         try
//         {
//             EnsureParsed();

//              if (_root is null)
//             throw new InvalidOperationException("No JSON loaded/parsed yet.");

//         var q = txtQuery.Text?.Trim();
//         if (string.IsNullOrWhiteSpace(q))
//             throw new InvalidOperationException("Enter a query like $.matchUpStats.visTeamName");

//         var results = JsonQueryEngine.Evaluate(_root, q);

//         // Format output
//         if (results.Count == 0)
//         {
//             txtOutput.Text = "(no results)";
//         }
//         else if (results.Count == 1)
//         {
//             txtOutput.Text = NodeToDisplay(results[0]);
//         }
//         else
//         {
//             // multiple results from wildcard
//             txtOutput.Text = string.Join(Environment.NewLine, results.Select(NodeToDisplay));
//         }

//         SetStatus($"Query OK ({results.Count} result(s))");
//     }
//     catch (Exception ex)
//     {
//         ShowError("Query failed", ex);
//     }
// }

private static string NodeToDisplay(JsonNode? n)
{
    if (n is null) return "null";

    // If it’s a value (string/number/bool/null), ToJsonString() is clean and consistent.
    // If it’s an object/array, this prints JSON.
    return n.ToJsonString(new System.Text.Json.JsonSerializerOptions
    {
        WriteIndented = true
    });
}


    // private void AddNode_Click(object sender, RoutedEventArgs e)
    // {
    //     try
    //     {
    //         EnsureParsed();
    //         // if (_selectedItem is null)
    //         //     throw new InvalidOperationException("Select a node in the JSON Tree first.");

    //         // var selectedNode = _selectedItem.Tag as JsonNode;
    //         if (selectedNode is null)
    //             throw new InvalidOperationException("Selected node is invalid.");

    //         string key = txtAddKey.Text?.Trim() ?? "";
    //         string rawValue = txtAddValue.Text?.Trim() ?? "";
    //         string type = GetSelectedType();

    //         // Create the new node
    //         JsonNode newNode = type switch
    //         {
    //             "string" => JsonValue.Create(rawValue)!,
    //             "number" => JsonValue.Create(ParseNumber(rawValue))!,
    //             "bool" => JsonValue.Create(ParseBool(rawValue))!,
    //             "null" => null!,
    //             "object" => new JsonObject(),
    //             "array" => new JsonArray(),
    //             _ => JsonValue.Create(rawValue)!
    //         };

    //         // Add to selected
    //         if (selectedNode is JsonObject obj)
    //         {
    //             if (string.IsNullOrWhiteSpace(key))
    //                 throw new ArgumentException("Key is required when adding to a JSON object.");

    //             obj[key] = newNode; // overwrite OK for simplicity
    //         }
    //         else if (selectedNode is JsonArray arr)
    //         {
    //             // Key ignored for arrays
    //             arr.Add(newNode);
    //         }
    //         else
    //         {
    //             throw new InvalidOperationException("You can only add to an Object or Array node.");
    //         }

    //         // Refresh tree
    //         RenderTree(_root!);
    //         SetStatus("Node added.");
    //     }
    //     catch (Exception ex)
    //     {
    //         ShowError("Add node failed", ex);
    //     }
    // }

    // private void Clear_Click(object sender, RoutedEventArgs e)
    // {
    //     _root = null;
    //     _selectedItem = null;

    //     txtSource.Text = "";
    //     txtJson.Text = "";
    //     txtOutput.Text = "";
    //     txtQuery.Text = "";
    //     txtQueryResult.Text = "";
    //     txtAddKey.Text = "";
    //     txtAddValue.Text = "";
    //     treeJson.Items.Clear();
    //     lblSelected.Text = "Selected: (none)";
    //     SetStatus("Cleared.");
    // }

    // private void TreeJson_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    // {
    //     _selectedItem = e.NewValue as TreeViewItem;
    //     if (_selectedItem?.Tag is JsonNode node)
    //     {
    //         lblSelected.Text = $"Selected: {_selectedItem.Header} ({NodeType(node)})";
    //     }
    //     else
    //     {
    //         lblSelected.Text = "Selected: (none)";
      
    //}

    // =========================
    // Tree Rendering
    // =========================

    // private void RenderTree(JsonNode root)
    // {
    //     treeJson.Items.Clear();
    //     var rootItem = BuildTreeItem("root", root);
    //     treeJson.Items.Add(rootItem);
    //     rootItem.IsExpanded = true;
    // }

    private TreeViewItem BuildTreeItem(string label, JsonNode node)
    {
        var item = new TreeViewItem
        {
            Header = label,
            Tag = node
        };

        if (node is JsonObject obj)
        {
            foreach (var kv in obj)
            {
                if (kv.Value is null)
                    item.Items.Add(new TreeViewItem { Header = $"{kv.Key}: null", Tag = JsonValue.Create((string?)null) });
                else
                    item.Items.Add(BuildTreeItem(kv.Key, kv.Value));
            }
        }
        else if (node is JsonArray arr)
        {
            for (int i = 0; i < arr.Count; i++)
            {
                var child = arr[i];
                if (child is null)
                    item.Items.Add(new TreeViewItem { Header = $"[{i}]: null", Tag = JsonValue.Create((string?)null) });
                else
                    item.Items.Add(BuildTreeItem($"[{i}]", child));
            }
        }
        else
        {
            // Value node
            item.Header = $"{label}: {node.ToJsonString()}";
        }

        return item;
    }

    // =========================
    // Query (simple path)
    // Supports: a.b.c and arr[0].x
    // =========================

    private JsonNode? QueryByPath(JsonNode root, string path)
    {
        JsonNode? current = root;
        var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            if (current is null) return null;

            // Handle "name" or "name[0]" or "[0]"
            string p = part.Trim();

            // If starts with [index]
            if (p.StartsWith("[") && p.EndsWith("]"))
            {
                int idx = int.Parse(p[1..^1]);
                if (current is JsonArray arr && idx >= 0 && idx < arr.Count) current = arr[idx];
                else return null;
                continue;
            }

            // name with optional [index]
            string name = p;
            int? index = null;

            int bracket = p.IndexOf('[');
            if (bracket >= 0 && p.EndsWith("]"))
            {
                name = p[..bracket];
                index = int.Parse(p[(bracket + 1)..^1]);
            }

            if (current is JsonObject obj)
            {
                current = obj[name];
            }
            else
            {
                return null;
            }

            if (index.HasValue)
            {
                if (current is JsonArray arr && index.Value >= 0 && index.Value < arr.Count) current = arr[index.Value];
                else return null;
            }
        }

        return current;
    }



    // private string GetSelectedType()
    // {
    //     if (cmbAddType.SelectedItem is ComboBoxItem item && item.Content is string s)
    //         return s;
    //     return "string";
    // }

    private static string NodeType(JsonNode node) =>
        node is JsonObject ? "Object" :
        node is JsonArray ? "Array" :
        "Value";

    private static decimal ParseNumber(string s)
    {
        if (!decimal.TryParse(s, out var n))
            throw new ArgumentException("Value is not a valid number.");
        return n;
    }

    private static bool ParseBool(string s)
    {
        if (!bool.TryParse(s, out var b))
            throw new ArgumentException("Value is not a valid bool (true/false).");
        return b;
    }
}
}