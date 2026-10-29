namespace MiniPharmacie.App.Forms;

internal static class PharmacyTheme
{
    public static readonly Color Forest = Color.FromArgb(24, 103, 76);
    public static readonly Color ForestDark = Color.FromArgb(17, 70, 54);
    public static readonly Color Leaf = Color.FromArgb(36, 139, 99);
    public static readonly Color Mint = Color.FromArgb(231, 244, 237);
    public static readonly Color Canvas = Color.FromArgb(246, 249, 247);
    public static readonly Color Surface = Color.White;
    public static readonly Color Ink = Color.FromArgb(32, 54, 45);
    public static readonly Color Muted = Color.FromArgb(103, 122, 112);
    public static readonly Color Border = Color.FromArgb(220, 231, 224);
    public static readonly Color Danger = Color.FromArgb(166, 63, 63);

    public static void Apply(Form form)
    {
        form.BackColor = Canvas;
        form.ForeColor = Ink;
        form.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

        ApplyToControls(form.Controls);
    }

    private static void ApplyToControls(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
            switch (control)
            {
                case Button button:
                    StyleButton(button);
                    break;
                case DataGridView grid:
                    StyleGrid(grid);
                    break;
                case TextBox textBox:
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    textBox.BackColor = Surface;
                    textBox.ForeColor = Ink;
                    textBox.Font = new Font("Segoe UI", 10F);
                    break;
                case ComboBox comboBox:
                    comboBox.FlatStyle = FlatStyle.Flat;
                    comboBox.BackColor = Surface;
                    comboBox.ForeColor = Ink;
                    comboBox.Font = new Font("Segoe UI", 10F);
                    break;
                case NumericUpDown numeric:
                    numeric.BorderStyle = BorderStyle.FixedSingle;
                    numeric.BackColor = Surface;
                    numeric.ForeColor = Ink;
                    numeric.Font = new Font("Segoe UI", 10F);
                    break;
                case DateTimePicker datePicker:
                    datePicker.CalendarMonthBackground = Surface;
                    datePicker.CalendarForeColor = Ink;
                    datePicker.Font = new Font("Segoe UI", 10F);
                    break;
                case Label label:
                    label.ForeColor = Ink;
                    break;
                case FlowLayoutPanel flow:
                    flow.BackColor = Surface;
                    break;
                case Panel panel when panel.BackColor == SystemColors.Control:
                    panel.BackColor = Surface;
                    break;
            }

            if (control.HasChildren)
                ApplyToControls(control.Controls);
        }
    }

    private static void StyleButton(Button button)
    {
        var text = button.Text.Trim();
        var isDanger = text.Contains("désactiv", StringComparison.OrdinalIgnoreCase)
            || text.Contains("supprim", StringComparison.OrdinalIgnoreCase)
            || text.Contains("restaurer", StringComparison.OrdinalIgnoreCase);
        var isPrimary = text.Contains("enregistrer", StringComparison.OrdinalIgnoreCase)
            || text.Contains("créer", StringComparison.OrdinalIgnoreCase)
            || text.Contains("connecter", StringComparison.OrdinalIgnoreCase)
            || text.Contains("encaisser", StringComparison.OrdinalIgnoreCase)
            || text.Contains("valider", StringComparison.OrdinalIgnoreCase)
            || text.Contains("ouvrir caisse", StringComparison.OrdinalIgnoreCase)
            || text.Contains("clôturer caisse", StringComparison.OrdinalIgnoreCase)
            || text.Contains("afficher", StringComparison.OrdinalIgnoreCase)
            || text.Contains("nouvelle vente", StringComparison.OrdinalIgnoreCase)
            || text.Contains("nouveau produit", StringComparison.OrdinalIgnoreCase)
            || text.Contains("sauvegarde vérifiée", StringComparison.OrdinalIgnoreCase);

        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = isPrimary || isDanger ? 0 : 1;
        button.FlatAppearance.BorderColor = Border;
        button.BackColor = isDanger ? Color.FromArgb(250, 238, 237) : isPrimary ? Leaf : Surface;
        button.ForeColor = isDanger ? Danger : isPrimary ? Color.White : ForestDark;
        button.Font = new Font("Segoe UI", 9.5F, isPrimary ? FontStyle.Bold : FontStyle.Regular);
        button.Cursor = Cursors.Hand;
        button.Padding = new Padding(8, 2, 8, 2);
    }

    private static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = Border;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersDefaultCellStyle.BackColor = ForestDark;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 4, 8, 4);
        grid.ColumnHeadersHeight = 42;
        grid.DefaultCellStyle.BackColor = Surface;
        grid.DefaultCellStyle.ForeColor = Ink;
        grid.DefaultCellStyle.SelectionBackColor = Mint;
        grid.DefaultCellStyle.SelectionForeColor = ForestDark;
        grid.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);
        grid.AlternatingRowsDefaultCellStyle.BackColor = Canvas;
        grid.RowTemplate.Height = 38;
        grid.RowHeadersVisible = false;
    }
}

public class PharmacyForm : Form
{
    protected override void OnShown(EventArgs e)
    {
        PharmacyTheme.Apply(this);
        base.OnShown(e);
    }
}
