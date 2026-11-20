import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent / '_lib'))
from docx import Document
from docx.shared import Pt, Cm
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.table import _Cell
from copy import deepcopy
from fractions import Fraction as F

OUT = Path(__file__).parent
digits = str.maketrans('0123456789', '۰۱۲۳۴۵۶۷۸۹')
def fa(s): return str(s).translate(digits)
def flag(parent, tag, val=None):
    e=OxmlElement('w:'+tag)
    if val is not None: e.set(qn('w:val'),str(val))
    parent.append(e)
    return e
def format_p(p, rtl=True, bold=False, after=0):
    p.alignment=WD_ALIGN_PARAGRAPH.RIGHT if rtl else WD_ALIGN_PARAGRAPH.LEFT
    pf=p.paragraph_format
    pf.space_before=Pt(0); pf.space_after=Pt(after)
    pf.line_spacing=Pt(21)
    flag(p._p.get_or_add_pPr(),'bidi','1' if rtl else '0')
    if rtl: p._p.get_or_add_pPr().find(qn('w:jc')).set(qn('w:val'),'start')
    for r in p.runs:
        r.font.name='B Nazanin'; r.font.size=Pt(14); r.bold=bold
        rp=r._r.get_or_add_rPr()
        fonts=rp.find(qn('w:rFonts'))
        for a in ['ascii','hAnsi','cs','eastAsia']: fonts.set(qn('w:'+a),'B Nazanin')
        flag(rp,'szCs','28'); flag(rp,'rtl','1' if rtl else '0')
        flag(rp,'bCs','1' if bold else '0')
    return p
def para(d,text='',rtl=True,bold=False,after=0):
    return format_p(d.add_paragraph(text),rtl,bold,after)
def math(d,text,after=0): return para(d,fa(text),False,False,after)
def blank(d,pt):
    p=para(d)
    p.paragraph_format.line_spacing=Pt(pt)
    p.paragraph_format.space_after=Pt(0)
def heading(d,n,text,score):
    para(d,fa(f'{n}ـ {text}    [{score} نمره]'),bold=True,after=3)
def base(answer=False):
    d=Document()
    s=d.sections[0]
    s.page_width=Cm(21); s.page_height=Cm(29.7)
    s.top_margin=Cm(1.1);s.bottom_margin=Cm(1.1)
    s.left_margin=Cm(1.3);s.right_margin=Cm(1.3)
    s.header_distance=Cm(.4);s.footer_distance=Cm(.4)
    normal=d.styles['Normal'];normal.font.name='B Nazanin';normal.font.size=Pt(14)
    # Preserve the supplied Word letterhead's three-cell, right-to-left structure.
    src=Document(OUT/'template.docx')
    t=deepcopy(src.tables[0]._tbl)
    for row in list(t.findall(qn('w:tr')))[1:]: t.remove(row)
    d._body._body.insert(0,t)
    table=d.tables[0]
    table.autofit=False
    vals=[
      ['نام و نام خانوادگی: ........................','نام پدر: ................    پایه: هفتم','نام دبیر ریاضی: طریقت'],
      ['مدیریت آموزش و پرورش شهرستان برخوار','کارشناسی سنجش و ارزشیابی تحصیلی','دبیرستان متوسطه اول ملکیان ۱'],
      ['نام درس: ریاضی ـ فصل ۲','مدت امتحان: ۹۰ دقیقه    کلاس: ....','تاریخ امتحان: ۱۴۰۵/۰۸/۲۴']]
    for tc,lines in zip(table.rows[0]._tr.tc_lst,vals):
        c=_Cell(tc,table)
        c.text='\n'.join(lines)
        for p in c.paragraphs: format_p(p,True,False)
    # Fit the original table to the printable width without changing its layout.
    for e in t.iter():
        if e.tag in [qn('w:tblW'),qn('w:tcW'),qn('w:gridCol')]:
            key=qn('w:w');v=e.get(key)
            if v and v.isdigit(): e.set(key,str(round(int(v)*10430/11348)))
        if e.tag==qn('w:tblInd'): e.set(qn('w:w'),'0')
    title='پاسخ‌نامهٔ تشریحی و راهنمای تصحیح' if answer else 'آزمون فصل دوم ریاضی هفتم ـ عددهای صحیح'
    p=para(d,title,bold=True,after=2);p.alignment=WD_ALIGN_PARAGRAPH.CENTER
    if not answer: para(d,'بارم کل: ۲۰ نمره | مراحل حل را بنویسید. محاسبات را از چپ به راست بخوانید.',after=4)
    f=s.footer.paragraphs[0]
    f.add_run('ملکیان ۱  |  ریاضی هفتم  |  صفحهٔ ')
    field=OxmlElement('w:fldSimple');field.set(qn('w:instr'),'PAGE');f._p.append(field)
    format_p(f);f.alignment=WD_ALIGN_PARAGRAPH.CENTER
    d.core_properties.title=title
    d.core_properties.author='دبیر ریاضی: طریقت'
    return d

expr=[
 '(-21 ÷ 3) × (-42 ÷ 6) × (-63 ÷ (-9)) + 7',
 '(-8 + 4) × (-8) × 4 × (-8 ÷ 4) + (-8 - 4)',
 '5 - 2 + (5 × (-2)) - (5 - (-2)) ÷ (2 - (-5))',
 '-(16 × 3) ÷ (-24 × 2) - (12 × (-5)) ÷ 15 - (-18)',
 '-16 ÷ 3 × (3 ÷ 16) × (-16 × 3) - (16 - (-3)) ÷ (-3 - 16)',
 '18 × 7 × (21 ÷ (-3)) ÷ 42 + (-91) ÷ 13 - (3 × (-17))',
 '512 ÷ (-2) ÷ (-2) ÷ (-2) ÷ (-2) ÷ (-2)',
 '8 × (-2) ÷ (-6) × (-2) ÷ (-4) + 11']
letters=['الف','ب','ج','د','هـ','و','ز','ح']
blanks=[['-4 + □ = -8','-10 + □ = 15','-8 + □ = -10'],
        ['10 + □ = -5','10 + □ = 15','10 + □ = -20'],
        ['2 - □ = -8','-3 - □ = +5','-4 - □ = -8'],
        ['-6 + □ = 15','4 - □ = -8','4 - □ = 10']]
blank_answers=[[ -4,25,-2],[-15,5,-30],[10,-8,4],[21,12,-6]]
elevator='شخصی در طبقهٔ ۵+ سوار آسان‌بر شد. او ابتدا ۳ طبقه بالا رفت و پس از انجام کاری ۶ طبقه پایین آمد و دوباره برای خرید چای ۲ طبقه پایین رفت و در همان طبقه هزینه‌های خرید خود را حساب کرد. سپس دوباره ۲ طبقه پایین رفت و در توقفگاه سوار ماشین خود شد. ماشین این شخص در کدام طبقه بود؟'
test='در یک آزمون تستی، برای هر پاسخ درست ۴ نمرهٔ مثبت و برای هر پاسخ نادرست ۲ نمرهٔ منفی در نظر می‌گیرند. دانش‌آموزی از ۷۰ پرسش آزمون، به ۵۶ پرسش پاسخ درست داده و به ۵ پرسش جواب نداده است. نمرهٔ نهایی این دانش‌آموز چند می‌شود؟ این نمره چقدر از نمرهٔ کامل کمتر است؟'
temp='اختلاف دمای دو جسم ۸ درجه است. اگر مجموع دمای این دو جسم ۲۰− درجه باشد، دمای هر یک چند درجه است؟'

d=base()
heading(d,1,'عددها را از کوچک‌تر به بزرگ‌تر و از چپ به راست مرتب کنید.',2)
math(d,'2 ، -1 ، 5 ، -15 ، 45 ، 0 ، -5 ، -34 ، 12 ، -23')
blank(d,35)
heading(d,2,'در جای خالی عدد مناسب بنویسید. (هر مورد ۰٫۲۵ نمره)',3)
t=d.add_table(rows=4,cols=3);t.autofit=False
flag(t._tbl.tblPr,'bidiVisual')
for row,vs in zip(t.rows,blanks):
    for c,v in zip(row.cells,vs):
        c.width=Cm(6.1);c.text=fa(v.replace('□','........'));format_p(c.paragraphs[0],False)
        c.paragraphs[0].paragraph_format.space_after=Pt(6)
blank(d,4)
heading(d,3,'مسئلهٔ آسان‌بر را حل کنید.',2)
para(d,elevator)
blank(d,43)
heading(d,4,'مسئلهٔ نمرهٔ آزمون را با نوشتن مراحل حل پاسخ دهید.',3)
para(d,test)
blank(d,54)
d.add_page_break()
para(d,'آزمون ریاضی هفتم ـ فصل ۲ | نام و نام خانوادگی: ........................',bold=True,after=8)
heading(d,5,'حاصل هر یک از عبارت‌های زیر را به دست آورید.',8)
para(d,'هر قسمت ۱ نمره دارد. مراحل حل هر قسمت را زیر همان عبارت بنویسید.',after=8)
for i in range(4):
    para(d,letters[i]+')',bold=True)
    math(d,expr[i])
    blank(d,83)
d.add_page_break()
para(d,'آزمون ریاضی هفتم ـ فصل ۲ | نام و نام خانوادگی: ........................',bold=True,after=8)
para(d,'ادامهٔ سؤال ۵ ـ هر قسمت ۱ نمره',bold=True,after=6)
for i in range(4,8):
    para(d,letters[i]+')',bold=True)
    math(d,expr[i])
    blank(d,65)
heading(d,6,'مسئلهٔ دما را حل کنید.',2)
para(d,temp)
blank(d,60)
para(d,'موفق باشید.',bold=True)
d.save(OUT/'exam.docx')

a=base(True)
para(a,'بارم کل: ۲۰ نمره | روش درستِ دیگر نیز با همین بارم پذیرفته شود.',after=7)
heading(a,1,'مرتب‌کردن عددهای صحیح',2)
math(a,'-34 < -23 < -15 < -5 < -1 < 0 < 2 < 5 < 12 < 45',after=3)
para(a,'بارم: ترتیب درست عددهای منفی ۱ نمره؛ جایگاه صفر و عددهای مثبت ۱ نمره.',after=6)
heading(a,2,'عددهای مناسب در جای خالی',3)
para(a,'پاسخ‌ها به ترتیب ردیف‌های جدول؛ در هر ردیف از ستون راست به چپ:')
for n,vs in enumerate(blank_answers,1):
    para(a,fa(f'ردیف {n}: ستون راست: {vs[0]} ؛ ستون میانی: {vs[1]} ؛ ستون چپ: {vs[2]}'))
para(a,'هر پاسخ درست ۰٫۲۵ نمره.',after=6)
heading(a,3,'مسئلهٔ آسان‌بر',2)
math(a,'(+5) + 3 - 6 - 2 - 2 = -2')
para(a,'ماشین در طبقهٔ ۲−، یعنی دو طبقه پایین‌تر از همکف، قرار دارد.')
para(a,'بارم: نوشتن عبارت درست ۱؛ محاسبه و تعیین طبقه ۱ نمره.',after=6)
heading(a,4,'مسئلهٔ نمرهٔ آزمون',3)
para(a,'تعداد پاسخ‌های نادرست:')
math(a,'70 - 56 - 5 = 9')
para(a,'نمرهٔ نهایی (پرسش‌های بی‌پاسخ نمره‌ای ندارند):')
math(a,'56 × 4 - 9 × 2 = 224 - 18 = 206')
para(a,'نمرهٔ کامل و مقدار کاهش نسبت به آن:')
math(a,'70 × 4 = 280     ;     280 - 206 = 74')
para(a,'نمرهٔ نهایی ۲۰۶ است و ۷۴ نمره از نمرهٔ کامل کمتر است.')
para(a,'بارم: تعداد نادرست ۰٫۵؛ نمرهٔ نهایی ۱٫۵؛ نمرهٔ کامل ۰٫۵؛ مقدار کاهش ۰٫۵.')
a.add_page_break()
heading(a,5,'محاسبهٔ عبارت‌ها ـ هر قسمت ۱ نمره',8)
steps=[
 ['(-7) × (-7) × 7 + 7 = 49 × 7 + 7 = 350'],
 ['(-4) × (-8) × 4 × (-2) + (-12)', ' = 32 × 4 × (-2) - 12 = -256 - 12 = -268'],
 ['5 - 2 - 10 - 7 ÷ 7 = 3 - 10 - 1 = -8'],
 ['(-48) ÷ (-48) - (-60) ÷ 15 + 18', ' = 1 - (-4) + 18 = 23'],
 ['(-16 ÷ 3) × (3 ÷ 16) = -1', '(-1) × (-48) - 19 ÷ (-19) = 48 - (-1) = 49'],
 ['18 × 7 × (-7) ÷ 42 - 7 - (-51)', ' = -21 - 7 + 51 = 23'],
 ['512 → -256 → 128 → -64 → 32 → -16', '512 ÷ (-2) ÷ (-2) ÷ (-2) ÷ (-2) ÷ (-2) = -16'],
 ['(-16) ÷ (-6) × (-2) ÷ (-4) + 11', ' = (8 ÷ 3) × (-2) ÷ (-4) + 11 = (4 ÷ 3) + 11 = 37 ÷ 3']]
for label,ss in zip(letters,steps):
    para(a,label+')',bold=True)
    for st in ss: math(a,st)
para(a,'بارم هر قسمت: رعایت ترتیب عملیات و محاسبات میانی ۰٫۷۵؛ پاسخ نهایی ۰٫۲۵.',after=6)
heading(a,6,'مسئلهٔ دما',2)
para(a,'دمای میانگین دو جسم ۱۰− است و هر دما ۴ درجه با میانگین فاصله دارد:')
math(a,'(-20) ÷ 2 = -10     ;     8 ÷ 2 = 4')
math(a,'-10 + 4 = -6     ;     -10 - 4 = -14')
para(a,'پاسخ: دمای دو جسم ۶− و ۱۴− درجه است. مجموع آن‌ها ۲۰− و اختلافشان ۸ است.')
para(a,'بارم: روش درست ۱؛ به‌دست‌آوردن هر دما ۰٫۵ نمره.')
a.save(OUT/'answer-key.docx')

# Independent exact-arithmetic checks for the eight transcribed expressions.
answers=[F(-21,3)*F(-42,6)*F(-63,-9)+7,(-8+4)*(-8)*4*F(-8,4)+(-8-4),
5-2+5*(-2)-F(5-(-2),2-(-5)),F(-(16*3),-24*2)-F(12*(-5),15)-(-18),
F(-16,3)*F(3,16)*(-16*3)-F(16-(-3),-3-16),18*7*F(21,-3)/42+F(-91,13)-3*(-17),
F(512,(-2)**5),F(8*(-2),-6)*(-2)/(-4)+11]
assert answers==[350,-268,-8,23,49,23,-16,F(37,3)],answers
assert 2+3+2+3+8+2==20
print('Created exam.docx and answer-key.docx; all eight exact answers and 20-point total verified.')
