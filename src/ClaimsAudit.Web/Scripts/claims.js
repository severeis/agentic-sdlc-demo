// Claims search box. Old-school jQuery on purpose: this mirrors the legacy front end.
$(function () {
    var $input = $('#claim-search');
    if (!$input.length) return;

    var timer = null;
    var url = $input.data('url');
    var $tbody = $('#claims-table tbody');

    function money(v) {
        return '$' + v;
    }

    function render(rows) {
        $tbody.empty();
        if (rows.length === 0) {
            $tbody.append('<tr><td colspan="9">No claims found</td></tr>');
            return;
        }
        $.each(rows, function (i, c) {
            var flags = $.map(c.Flags, function (f) { return '<span class="flag">' + f + '</span>'; }).join(' ');
            var tr = '<tr>' +
                '<td><a class="claim-id" href="/Claims/Details/' + c.Id + '">' + c.Id + '</a></td>' +
                '<td>' + c.ServiceDate + '</td>' +
                '<td>' + c.PatientId + '</td>' +
                '<td>' + c.ProviderName + '</td>' +
                '<td><span class="code">' + c.CptCode + '</span></td>' +
                '<td class="num">' + c.Units + '</td>' +
                '<td class="num">' + money(c.BilledAmount) + '</td>' +
                '<td><span class="status status-' + String(c.Status).toLowerCase() + '">' + c.Status + '</span></td>' +
                '<td>' + flags + '</td>' +
                '</tr>';
            $tbody.append(tr);
        });
    }

    $input.on('keyup', function () {
        clearTimeout(timer);
        var q = $input.val();
        timer = setTimeout(function () {
            $.getJSON(url, { q: q }, render);
        }, 250);
    });
});
