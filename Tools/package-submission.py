"""Build a source-only archive without credentials, caches, historical copies, or dependencies."""
from pathlib import Path
from zipfile import ZipFile,ZIP_DEFLATED
import os,json,hashlib

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'output'
EXCLUDED={'.git','.vs','.vscode','.idea','node_modules','node_modules.cra-backup','bin','obj','build',
          'test-results','playwright-report','TestResults','tmp','.tools','output','Logs','Github','Dokumentim-Travel-Agency','__pycache__'}
ARCHIVE=OUT/'Travel_Agency_Submission.zip'
def include(path):
    rel=path.relative_to(ROOT)
    if path.is_symlink() or not path.resolve().is_relative_to(ROOT):return False
    if any(part in EXCLUDED for part in rel.parts):return False
    name=path.name.lower()
    if name.startswith('.env') and name!='.env.example':return False
    if name in {'appsettings.local.json','package-lock.cra-backup.json','requirements_pdf_text.txt','1.html','rct.txt'}:return False
    if name.endswith(('.log','.zip','.rar','.pyc','.user','.trx')):return False
    if name.endswith('.docx'):return False
    if name.endswith('.txt') and name!='requirements.txt':return False
    return True

def main():
    OUT.mkdir(exist_ok=True)
    files=[]
    for current,dirs,names in os.walk(ROOT):
        dirs[:]=sorted(d for d in dirs if d not in EXCLUDED and not (Path(current)/d).is_symlink())
        for name in sorted(names):
            p=Path(current)/name
            if include(p):files.append(p)
    manifest=[]
    with ZipFile(ARCHIVE,'w',ZIP_DEFLATED,compresslevel=6) as archive:
        for p in files:
            data=p.read_bytes();name=p.relative_to(ROOT).as_posix()
            manifest.append({'path':name,'bytes':len(data),'sha256':hashlib.sha256(data).hexdigest()})
            archive.writestr('Travel-Agency/'+name,data)
        archive.writestr('Travel-Agency/SUBMISSION_MANIFEST.json',json.dumps(manifest,indent=2)+'\n')
    with ZipFile(ARCHIVE) as check:
        assert check.testzip() is None
        assert all(not any(part in EXCLUDED for part in Path(n).parts) for n in check.namelist())
        assert not any(Path(n).name=='.env' for n in check.namelist())
        assert 'Travel-Agency/.github/workflows/deploy-staging.yml' in check.namelist()
        assert 'Travel-Agency/UI/travel-agency/src/App.jsx' in check.namelist()
    (OUT/'submission-manifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
    print(f'{ARCHIVE}\n{len(files)} source/documentation files; {ARCHIVE.stat().st_size:,} bytes; integrity verified')

if __name__=='__main__':main()
