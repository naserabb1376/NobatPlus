param(
    [string]$ChromePath = 'C:\Program Files\Google\Chrome\Application\chrome.exe',
    [switch]$KeepPreview
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$sourcePath = Join-Path $root 'financial-guide.fa.md'
$htmlPath = Join-Path $root '.financial-guide.preview.html'
$pdfPath = Join-Path $root 'financial-guide.fa.pdf'
$profile = Join-Path $env:TEMP ('nobatix-guide-' + [Guid]::NewGuid().ToString('N'))
$utf8 = New-Object System.Text.UTF8Encoding($false)

function Encode([string]$value) {
    return [System.Net.WebUtility]::HtmlEncode($value)
}

function Inline([string]$value) {
    $encoded = Encode $value
    return [regex]::Replace($encoded, '`([^`]+)`', '<code dir="ltr">$1</code>')
}

$lines = [System.IO.File]::ReadAllLines($sourcePath, $utf8)
$body = New-Object System.Text.StringBuilder
$toc = New-Object System.Text.StringBuilder
$inCode = $false
$inTable = $false
$inList = $false
$codeLines = New-Object 'System.Collections.Generic.List[string]'
$heading = 0

foreach ($line in $lines) {
    if ($line.StartsWith('```')) {
        if ($inCode) {
            [void]$body.Append('<pre dir="ltr"><code>' + (Encode ($codeLines -join "`n")) + '</code></pre>')
            $codeLines.Clear()
        }
        $inCode = -not $inCode
        continue
    }
    if ($inCode) { $codeLines.Add($line); continue }

    if ($inTable -and -not $line.StartsWith('|')) {
        [void]$body.Append('</tbody></table></div>')
        $inTable = $false
    }
    if ($inList -and $line -notmatch '^(- |\d+\. )') {
        [void]$body.Append('</ul>')
        $inList = $false
    }
    if ([string]::IsNullOrWhiteSpace($line)) { continue }

    if ($line -match '^(#{1,4}) (.+)$') {
        $level = $matches[1].Length
        $title = $matches[2]
        $heading++
        $id = 'section-' + $heading
        if ($level -eq 2) {
            [void]$toc.Append('<li><a href="#' + $id + '">' + (Inline $title) + '</a></li>')
        }
        if ($level -ne 1) {
            [void]$body.Append('<h' + $level + ' id="' + $id + '">' + (Inline $title) + '</h' + $level + '>')
        }
        continue
    }

    if ($line.StartsWith('|')) {
        $cells = $line.Trim().Trim('|').Split('|')
        if ($line -match '^\|[\s:|\-]+\|$') { continue }
        $tag = 'td'
        if (-not $inTable) {
            [void]$body.Append('<div class="table-wrap"><table><tbody>')
            $inTable = $true
            $tag = 'th'
        }
        [void]$body.Append('<tr>')
        foreach ($cell in $cells) {
            [void]$body.Append('<' + $tag + ' dir="auto">' + (Inline $cell.Trim()) + '</' + $tag + '>')
        }
        [void]$body.Append('</tr>')
        continue
    }

    if ($line -match '^(- |\d+\. )(.+)$') {
        if (-not $inList) { [void]$body.Append('<ul>'); $inList = $true }
        [void]$body.Append('<li>' + (Inline $matches[2]) + '</li>')
        continue
    }
    [void]$body.Append('<p>' + (Inline $line) + '</p>')
}
if ($inList) { [void]$body.Append('</ul>') }
if ($inTable) { [void]$body.Append('</tbody></table></div>') }
if ($inCode) { throw 'Unclosed fenced code block.' }

$html = @"
<!doctype html>
<html lang="fa" dir="rtl">
<head>
<meta charset="utf-8">
<title>Nobatix | Financial Architecture and Frontend Guide | 2026-11-07</title>
<style>
@page { size: A4; margin: 17mm 16mm 20mm;
    @bottom-center { content: counter(page); font: 9pt Tahoma, sans-serif; color: #64706f; }
}
* { box-sizing: border-box; }
html { background: #e8eded; }
body { max-width: 210mm; margin: 24px auto; padding: 17mm 16mm; background: white;
    color: #222b2a; font: 10pt/1.9 Tahoma, Arial, sans-serif; letter-spacing: 0; }
.cover { min-height: 240mm; border-top: 6px solid #12635b; padding-top: 35mm; break-after: page; }
.brand { color: #12635b; font: bold 20pt Arial,sans-serif; direction: ltr; text-align: right; }
.cover h1 { font-size: 25pt; line-height: 1.8; margin: 16mm 0 6mm; }
.cover .subtitle { font-size: 13pt; line-height: 2; max-width: 140mm; }
.cover .meta { margin-top: 30mm; color: #56615f; }
.toc { break-after: page; }
.toc ul { list-style: none; padding: 0; columns: 1; }
.toc li { padding: 2px 0; border-bottom: 1px solid #e4e9e8; font-size: 10pt; }
a { color: #12635b; text-decoration: none; }
h2 { font-size: 16pt; color: #145c55; border-bottom: 1px solid #c9d7d4;
    padding-bottom: 3mm; margin-top: 9mm; break-after: avoid; }
h3 { font-size: 12pt; margin: 7mm 0 3mm; break-after: avoid; }
h4 { font-size: 11pt; direction: ltr; text-align: right; margin: 5mm 0 2mm; break-after: avoid; }
p { margin: 0 0 3mm; overflow-wrap: anywhere; orphans: 3; widows: 3; }
li { margin-bottom: 1.5mm; overflow-wrap: anywhere; }
ul { padding-right: 6mm; }
pre { text-align: left; background: #f2f5f6; border-left: 3px solid #537b82;
    padding: 3mm; font: 8pt/1.6 Consolas, 'Courier New', Tahoma, monospace;
    white-space: pre-wrap; overflow-wrap: anywhere; break-inside: avoid; unicode-bidi: isolate; }
code { font-family: Consolas, 'Courier New', Tahoma, monospace; font-size: .93em; unicode-bidi: isolate; }
.table-wrap { margin: 4mm 0; }
table { width: 100%; border-collapse: collapse; table-layout: auto; font-size: 8.3pt; line-height: 1.8; }
th { background: #e5efec; font-weight: bold; }
td, th { border: 1px solid #cbd6d3; padding: 1.8mm 2mm; text-align: start;
    overflow-wrap: anywhere; word-break: normal; max-width: 110mm; }
tr { break-inside: avoid; }
@media print { html { background: white; } body { margin: 0; padding: 0; max-width: none; }
    h2 { break-before: page; } main > h2:first-of-type { break-before: auto; }
    .cover { min-height: 230mm; }
}
</style>
</head>
<body>
<section class="cover">
<div class="brand">NOBATIX</div>
<h1>راهنمای جامع سیستم مالی</h1>
<p class="subtitle">معماری و قواعد مالی، قرارداد API، راهنمای پیاده‌سازی فرانت‌اند، استقرار و سناریوهای آزمون</p>
<div class="meta"><p>بازسازی از روی سورس فعلی پروژه</p><p>نسخه ۲۰۲۶/۱۱/۰۷</p><p>اشتراک · کارمزد · اعتبار · صورتحساب · درگاه · بازپرداخت</p></div>
</section>
<nav class="toc"><h2>فهرست مطالب</h2><ul>$toc</ul></nav>
<main>$body</main>
</body></html>
"@

try {
    [System.IO.File]::WriteAllText($htmlPath, $html, $utf8)
    $url = [System.Uri]::new($htmlPath).AbsoluteUri
    $arguments = @('--headless=new', '--disable-gpu', '--no-pdf-header-footer', '--allow-file-access-from-files',
        '--export-tagged-pdf', '--generate-pdf-document-outline', '--run-all-compositor-stages-before-draw',
        ('--user-data-dir="' + $profile + '"'), ('--print-to-pdf="' + $pdfPath + '"'), $url)
    $process = Start-Process -FilePath $ChromePath -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
    if ($process.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $pdfPath)) { throw 'PDF export failed.' }
    Get-Item -LiteralPath $pdfPath | Select-Object FullName, Length, LastWriteTime
}
finally {
    if (-not $KeepPreview -and (Test-Path -LiteralPath $htmlPath)) { Remove-Item -LiteralPath $htmlPath -Force }
    # Only the unique temporary Chrome profile created by this export is removed.
    $tempRoot = [System.IO.Path]::GetFullPath($env:TEMP).TrimEnd('\') + '\'
    $resolvedProfile = [System.IO.Path]::GetFullPath($profile)
    if ($resolvedProfile.StartsWith($tempRoot, [System.StringComparison]::OrdinalIgnoreCase) -and
        [System.IO.Path]::GetFileName($resolvedProfile).StartsWith('nobatix-guide-') -and
        (Test-Path -LiteralPath $resolvedProfile)) {
        Remove-Item -LiteralPath $resolvedProfile -Recurse -Force -ErrorAction SilentlyContinue
    }
}
