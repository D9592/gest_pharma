using MiniPharmacie.Business.Models;
using MiniPharmacie.Business.Services;
using MiniPharmacie.Data.Database;
using MiniPharmacie.App.Forms;

namespace MiniPharmacie.App;

public sealed class MainForm : Form
{
    private readonly long userId;
    private readonly bool manager;
    private readonly ReportingService reports = new(new DatabaseInitializer());
    private readonly System.Windows.Forms.Timer backupTimer = new() { Interval = 60_000 };
    private readonly Label salesValue = MetricValue("0,00");
    private readonly Label outOfStockValue = MetricValue("0");
    private readonly Label minimumValue = MetricValue("0");
    private readonly Label expiryValue = MetricValue("0");
    private bool backupRunning;

    public MainForm(long authenticatedUserId)
    {
        userId = authenticatedUserId;
        manager = IsManager(userId);

        Text = "Mini-pharmacie | Tableau de bord";
        MinimumSize = new Size(1000, 680);
        Size = new Size(1280, 820);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = PharmacyTheme.Canvas;
        ForeColor = PharmacyTheme.Ink;
        Font = new Font("Segoe UI", 10F);

        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = PharmacyTheme.Canvas
        };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 246));
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(shell);

        var sidebar = BuildSidebar();
        shell.Controls.Add(sidebar, 0, 0);

        var workspace = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = PharmacyTheme.Canvas
        };
        workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        workspace.Controls.Add(BuildTopBar(), 0, 0);
        workspace.Controls.Add(BuildDashboardContent(), 0, 1);
        shell.Controls.Add(workspace, 1, 0);

        Shown += async (_, _) =>
        {
            await LoadDashboard();
            if (manager)
            {
                backupTimer.Tick += async (_, _) => await RunScheduledBackup();
                backupTimer.Start();
                await RunScheduledBackup();
            }
        };
        FormClosed += (_, _) => backupTimer.Dispose();
    }

    private Control BuildSidebar()
    {
        var sidebar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = PharmacyTheme.ForestDark,
            Padding = new Padding(18, 22, 18, 16)
        };
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 105));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));

        var brand = new Panel { Dock = DockStyle.Fill, BackColor = PharmacyTheme.ForestDark };
        brand.Controls.Add(new Label
        {
            Text = "MINI-PHARMACIE",
            Location = new Point(2, 8),
            Size = new Size(194, 32),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        });
        brand.Controls.Add(new Label
        {
            Text = "GESTION D’OFFICINE",
            Location = new Point(2, 42),
            Size = new Size(194, 24),
            ForeColor = Color.FromArgb(183, 218, 199),
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        });
        sidebar.Controls.Add(brand, 0, 0);

        var nav = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = false,
            BackColor = PharmacyTheme.ForestDark,
            Padding = new Padding(0, 6, 0, 0)
        };
        AddNavigationButton(nav, "Ventes et caisse", () => ShowDialog(new SalesForm(userId)));
        AddNavigationButton(nav, "Produits", () => ShowDialog(new ProductsForm(userId)));
        AddNavigationButton(nav, "Mes rapports", () => ShowDialog(new ReportsForm(userId, manager)));
        if (manager)
        {
            AddNavigationButton(nav, "Utilisateurs", () => ShowDialog(new UsersForm(userId)));
            AddNavigationButton(nav, "Paramètres", () => ShowDialog(new SettingsForm(userId)));
            AddNavigationButton(nav, "Sauvegardes", () => ShowDialog(new BackupForm(userId)));
        }
        sidebar.Controls.Add(nav, 0, 1);

        var identity = new Panel { Dock = DockStyle.Fill, BackColor = PharmacyTheme.ForestDark, Padding = new Padding(2, 8, 0, 0) };
        identity.Controls.Add(new Label
        {
            Text = "Compte sécurisé",
            Dock = DockStyle.Top,
            Height = 22,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F),
            TextAlign = ContentAlignment.MiddleLeft
        });
        identity.Controls.Add(new Label
        {
            Text = manager ? "SESSION RESPONSABLE" : "SESSION VENDEUR",
            Dock = DockStyle.Top,
            Height = 25,
            ForeColor = Color.FromArgb(183, 218, 199),
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        });
        sidebar.Controls.Add(identity, 0, 2);
        return sidebar;
    }

    private void AddNavigationButton(FlowLayoutPanel nav, string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            Width = 190,
            Height = 44,
            Margin = new Padding(0, 3, 0, 5),
            FlatStyle = FlatStyle.Flat,
            BackColor = PharmacyTheme.ForestDark,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5F),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 8, 0),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(65, 116, 94);
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = PharmacyTheme.Forest;
        button.Click += (_, _) => action();
        nav.Controls.Add(button);
    }

    private Control BuildTopBar()
    {
        var topBar = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30, 0, 34, 0)
        };
        topBar.Controls.Add(new Label
        {
            Text = "Accueil   /   Tableau de bord",
            Dock = DockStyle.Left,
            AutoSize = false,
            Width = 320,
            ForeColor = PharmacyTheme.Muted,
            Font = new Font("Segoe UI", 9.5F),
            TextAlign = ContentAlignment.MiddleLeft
        });
        var role = new Label
        {
            Text = manager ? "RESPONSABLE" : "VENDEUR",
            Dock = DockStyle.Right,
            AutoSize = false,
            Width = 128,
            ForeColor = PharmacyTheme.Forest,
            BackColor = PharmacyTheme.Mint,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0, 14, 0, 14)
        };
        topBar.Controls.Add(role);
        return topBar;
    }

    private Control BuildDashboardContent()
    {
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = PharmacyTheme.Canvas };
        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(30, 24, 30, 28),
            BackColor = PharmacyTheme.Canvas
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 174));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 168));

        var intro = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = PharmacyTheme.Canvas,
            Padding = new Padding(0, 0, 0, 10)
        };
        intro.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        intro.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        intro.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        intro.Controls.Add(new Label
        {
            Text = "VOTRE OFFICINE, EN UN COUP D’ŒIL",
            Dock = DockStyle.Fill,
            ForeColor = PharmacyTheme.Leaf,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);
        intro.Controls.Add(new Label
        {
            Text = "La santé de votre pharmacie,\nen toute maîtrise.",
            Dock = DockStyle.Fill,
            ForeColor = PharmacyTheme.ForestDark,
            Font = new Font("Georgia", 22F, FontStyle.Bold | FontStyle.Italic),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 1);
        intro.Controls.Add(new Label
        {
            Text = "Suivez les ventes, surveillez les stocks et gardez les échéances importantes à portée de main.",
            Dock = DockStyle.Fill,
            ForeColor = PharmacyTheme.Muted,
            Font = new Font("Segoe UI", 9.5F),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 2);
        content.Controls.Add(intro, 0, 0);

        var metrics = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = PharmacyTheme.Canvas,
            Margin = new Padding(-6, 0, -6, 0)
        };
        for (var i = 0; i < 4; i++) metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        metrics.Controls.Add(CreateMetricCard("VENTES DU JOUR", salesValue, "Montant encaissé aujourd’hui"), 0, 0);
        metrics.Controls.Add(CreateMetricCard("EN RUPTURE", outOfStockValue, "Produits sans stock vendable"), 1, 0);
        metrics.Controls.Add(CreateMetricCard("STOCK À SUIVRE", minimumValue, "Produits au seuil minimum"), 2, 0);
        metrics.Controls.Add(CreateMetricCard("PÉREMPTION", expiryValue, "Lots proches de leur échéance"), 3, 0);
        content.Controls.Add(metrics, 0, 1);

        content.Controls.Add(new Label
        {
            Text = "Accès rapide",
            Dock = DockStyle.Fill,
            ForeColor = PharmacyTheme.ForestDark,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 2, 0, 0)
        }, 0, 2);

        var actions = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = manager ? 2 : 1,
            RowCount = 1,
            BackColor = PharmacyTheme.Canvas,
            Margin = new Padding(-6, 0, -6, 0)
        };
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, manager ? 50 : 100));
        if (manager) actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        actions.Controls.Add(CreateActionCard(
            "Vendre au comptoir",
            "Recherchez un médicament, préparez le panier et encaissez la vente.",
            "Démarrer une vente",
            () => ShowDialog(new SalesForm(userId))), 0, 0);
        if (manager)
        {
            actions.Controls.Add(CreateActionCard(
                "Gérer le catalogue",
                "Consultez les produits et mettez à jour les références de l’officine.",
                "Voir les produits",
                () => ShowDialog(new ProductsForm(userId))), 1, 0);
        }
        content.Controls.Add(actions, 0, 3);

        scroll.Controls.Add(content);
        scroll.SizeChanged += (_, _) => content.Width = Math.Max(720, scroll.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        content.Width = Math.Max(720, scroll.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        return scroll;
    }

    private static Control CreateMetricCard(string title, Label value, string note)
    {
        var card = CreateCard();
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.White,
            Padding = new Padding(14, 10, 12, 8)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            ForeColor = PharmacyTheme.Muted,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);
        value.Dock = DockStyle.Fill;
        value.TextAlign = ContentAlignment.MiddleLeft;
        layout.Controls.Add(value, 0, 1);
        layout.Controls.Add(new Label
        {
            Text = note,
            Dock = DockStyle.Fill,
            ForeColor = PharmacyTheme.Muted,
            Font = new Font("Segoe UI", 8.5F),
            TextAlign = ContentAlignment.TopLeft
        }, 0, 2);
        card.Controls.Add(layout);
        return card;
    }

    private static Control CreateActionCard(string title, string description, string buttonText, Action action)
    {
        var card = CreateCard();
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Color.White,
            Padding = new Padding(16, 10, 14, 10)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 176));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 33));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            ForeColor = PharmacyTheme.ForestDark,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);
        layout.SetColumnSpan(layout.GetControlFromPosition(0, 0)!, 2);
        layout.Controls.Add(new Label
        {
            Text = description,
            Dock = DockStyle.Fill,
            ForeColor = PharmacyTheme.Muted,
            Font = new Font("Segoe UI", 9F),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 1);
        var button = new Button
        {
            Text = buttonText,
            Dock = DockStyle.Fill,
            BackColor = PharmacyTheme.Leaf,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(8, 7, 0, 7)
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = PharmacyTheme.Forest;
        button.Click += (_, _) => action();
        layout.Controls.Add(button, 1, 1);
        card.Controls.Add(layout);
        return card;
    }

    private static Panel CreateCard()
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(6),
            Padding = Padding.Empty
        };
        card.Paint += (_, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle,
            PharmacyTheme.Border, ButtonBorderStyle.Solid);
        return card;
    }

    private static Label MetricValue(string text) => new()
    {
        Text = text,
        ForeColor = PharmacyTheme.Forest,
        Font = new Font("Segoe UI", 22F, FontStyle.Bold),
        AutoEllipsis = true
    };

    private void ShowDialog(Form form)
    {
        using (form) form.ShowDialog(this);
        _ = LoadDashboard();
    }

    private async Task RunScheduledBackup()
    {
        if (backupRunning) return;
        backupRunning = true;
        try
        {
            var scheduled = await new DailyBackupScheduler(new DatabaseInitializer(), new DatabaseBackupService(new DatabaseInitializer())).RunIfDueAsync(userId);
            if (scheduled is not null) Text = "Mini-pharmacie | Tableau de bord — sauvegarde vérifiée";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Sauvegarde automatique non terminée : {ex.Message}", "Sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally { backupRunning = false; }
    }

    private async Task LoadDashboard()
    {
        try
        {
            var snapshot = await reports.GetDashboardAsync();
            salesValue.Text = (snapshot.SalesTodayMinor / 100m).ToString("N2");
            outOfStockValue.Text = snapshot.OutOfStock.ToString("N0");
            minimumValue.Text = snapshot.AtMinimum.ToString("N0");
            expiryValue.Text = snapshot.ExpiringLots.ToString("N0");
        }
        catch
        {
            salesValue.Text = "—";
            outOfStockValue.Text = "—";
            minimumValue.Text = "—";
            expiryValue.Text = "—";
            MessageBox.Show("Le tableau de bord ne peut pas charger les indicateurs. Vérifiez la connexion à la base de données.", "Tableau de bord indisponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static bool IsManager(long id)
    {
        using var connection = new DatabaseInitializer().CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT r.Name FROM Users u JOIN Roles r ON r.Id=u.RoleId WHERE u.Id=$u AND u.IsActive=1";
        command.Parameters.AddWithValue("$u", id);
        return string.Equals(Convert.ToString(command.ExecuteScalar()), "Responsable", StringComparison.OrdinalIgnoreCase);
    }
}
