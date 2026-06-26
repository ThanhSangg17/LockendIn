const fs = require('fs');
const path = require('path');

function walkDir(dir, callback) {
  fs.readdirSync(dir).forEach(f => {
    let dirPath = path.join(dir, f);
    if (fs.statSync(dirPath).isDirectory()) {
      walkDir(dirPath, callback);
    } else if (f.endsWith('.tsx')) {
      callback(dirPath);
    }
  });
}

const targetClasses = [
  'bg-brand-red', 'bg-brand-surface', 'bg-white/5', 'bg-gradient-to-r', 'border', 'bg-brand-dark'
];

const excludeClasses = [
  'rounded-', 'min-h-screen', 'inset-0', 'bg-brand-black', 'border-b', 'border-t', 'border-l', 'border-r', 'bg-transparent', 'divide-', 'h-px', 'w-px', 'h-0.5', 'w-0.5', 'h-1', 'w-1', 'bottom-0'
];

walkDir('./src', (filePath) => {
  let content = fs.readFileSync(filePath, 'utf8');
  let modified = false;

  // Regex to find className="<classes>"
  const classRegex = /className="([^"]+)"/g;
  
  content = content.replace(classRegex, (match, classList) => {
    // Check if it has any target class
    const hasTarget = targetClasses.some(c => classList.split(/\s+/).includes(c) || classList.includes('bg-gradient-to'));
    
    if (hasTarget) {
      // Check if it shouldn't be excluded
      const shouldExclude = excludeClasses.some(c => classList.includes(c));
      
      if (!shouldExclude) {
        // Decide what to add. If it has 'w-4 h-4' or 'w-3 h-3' or 'w-10 h-10', maybe full or xl
        let addClass = 'rounded-xl';
        if (classList.includes('w-3 h-3') || classList.includes('w-4 h-4')) {
           addClass = 'rounded-full';
        } else if (classList.includes('bg-brand-surface') || classList.includes('bg-brand-dark') || classList.includes('bg-gradient-to')) {
           addClass = 'rounded-2xl';
        }
        
        modified = true;
        return `className="${classList} ${addClass}"`;
      }
    }
    return match;
  });

  // Specifically fix AdminHome's rounded-none if any
  if (content.includes('rounded-none')) {
    content = content.replace(/rounded-none/g, 'rounded-2xl');
    modified = true;
  }
  if (content.includes('rounded-sm')) {
    content = content.replace(/rounded-sm/g, 'rounded-md');
    modified = true;
  }

  if (modified) {
    fs.writeFileSync(filePath, content, 'utf8');
    console.log('Updated', filePath);
  }
});
