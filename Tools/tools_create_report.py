from pathlib import Path
import re,shutil
from docx import Document
from docx.shared import Inches,Pt,RGBColor
from docx.oxml import OxmlElement
from docx.oxml.ns import qn

ROOT=Path(__file__).resolve().parents[1]

def render_markdown(source,output):
    doc=Document()
    sec=doc.sections[0]
    sec.page_width=Inches(8.5);sec.page_height=Inches(11)
    sec.top_margin=sec.bottom_margin=Inches(.8)
    sec.left_margin=sec.right_margin=Inches(.85)
    normal=doc.styles['Normal'];normal.font.name='Calibri';normal.font.size=Pt(11)
    normal.paragraph_format.space_after=Pt(7)
    normal.paragraph_format.line_spacing=1.12
    for name in ['Title','Subtitle','Heading 1','Heading 2','Heading 3']:
        style=doc.styles[name];style.font.color.rgb=RGBColor(0,0,0)
        style.font.name='Calibri'
    title=doc.styles['Title'];title.font.size=Pt(26)
    for name in ['Heading 1','Heading 2']:
        doc.styles[name].paragraph_format.keep_with_next=True
    footer=sec.footer.paragraphs[0];footer.alignment=2
    footer.add_run('Travel Agency   |   ')
    field=OxmlElement('w:fldSimple');field.set(qn('w:instr'),'PAGE');footer._p.append(field)
    text=source.read_text(encoding='utf-8')
    for line in text.splitlines():
        if not line.strip():continue
        if line.startswith('# '):
            doc.add_paragraph(line[2:],style='Title')
            doc.add_paragraph('Web Services and Web APIs\nVerified local revision   16 November 2026',style='Subtitle')
        elif line.startswith('## '):doc.add_heading(line[3:],level=1)
        elif line.startswith('### '):doc.add_heading(line[4:],level=2)
        elif re.match(r'^\d+\. ',line):doc.add_paragraph(re.sub(r'^\d+\. ','',line),style='List Number')
        elif line.startswith('- '):doc.add_paragraph(line[2:],style='List Bullet')
        else:doc.add_paragraph(line.replace('`',''))
    doc.core_properties.title=doc.paragraphs[0].text
    doc.core_properties.author='Travel Agency project team to confirm'
    output.parent.mkdir(parents=True,exist_ok=True);doc.save(output)
    return output

if __name__=='__main__':
    report=render_markdown(ROOT/'Documentation/PROJECT_REPORT.md',ROOT/'Documentation/Generated/Travel_Agency_Raport_Teknik.docx')
    shutil.copyfile(report,ROOT/'Travel_Agency_Raport_Teknik.docx')
    presentation=render_markdown(ROOT/'Documentation/PRESENTATION_GUIDE.md',ROOT/'Travel_Agency_Final_Presentation.docx')
    print(report);print(presentation)
