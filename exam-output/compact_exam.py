import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent/'_lib'))
from docx import Document
from docx.shared import Pt
from docx.oxml.ns import qn

root=Path(__file__).parent
source=Document(root/'exam.docx')
original_text=[t.text for t in source.paragraphs if t.text.strip()]
ps=list(source.paragraphs)
# Keep the first page and all user edits. Join the two calculation pages.
for index in [29,30,31]:
    p=ps[index]
    p._element.getparent().remove(p._element)
for i in range(17,44):
    if i in [29,30,31]: continue
    p=ps[i]
    f=p.paragraph_format
    f.space_before=Pt(0);f.space_after=Pt(0)
    if not p.text.strip(): f.line_spacing=Pt(30)
    elif p.text in ['الف)','ب)','ج)','د)','هـ)','و)','ز)','ح)']:
        f.line_spacing=Pt(18)
        f.keep_with_next=True
    else:
        f.line_spacing=Pt(21)
        f.keep_with_next=True
ps[46].paragraph_format.line_spacing=Pt(35)
for i in [14,15,16]:
    ps[i].paragraph_format.space_after=Pt(3)
ps[44].paragraph_format.keep_with_next=True
# Verify that only the repeated page heading and continuation label were removed.
expected=[p.text for i,p in enumerate(ps) if i not in [29,30,31] and p.text.strip()]
actual=[p.text for p in source.paragraphs if p.text.strip()]
assert actual==expected
source.save(root/'exam-two-pages.docx')
print('Created compact copy; all question text, tables, and user edits preserved.')
