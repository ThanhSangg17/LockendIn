const fs = require('fs');
const path = require('path');

function walkDir(dir, callback) {
  fs.readdirSync(dir).forEach(f => {
    let dirPath = path.join(dir, f);
    let isDirectory = fs.statSync(dirPath).isDirectory();
    if (isDirectory) {
      walkDir(dirPath, callback);
    } else if (f.endsWith('.tsx') || f.endsWith('.css')) {
      callback(path.join(dir, f));
    }
  });
}

const sharpRegex = /className="[^"]*\b(bg-[a-zA-Z0-9-]+|border)\b[^"]*"/g;

walkDir('./src', (filePath) => {
  const content = fs.readFileSync(filePath, 'utf8');
  let match;
  while ((match = sharpRegex.exec(content)) !== null) {
    const cls = match[0];
    if (!cls.includes('rounded-') && !cls.includes('rounded-full') && !cls.includes('border-b') && !cls.includes('border-t') && !cls.includes('bg-transparent')) {
      // Just some heuristics to exclude full screen containers
      if (!cls.includes('min-h-screen') && !cls.includes('inset-0') && !cls.includes('bg-brand-black') && !cls.includes('border-brand-border')) {
         console.log(filePath + ' -> ' + cls);
      }
    }
  }
});
