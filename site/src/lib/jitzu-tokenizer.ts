import { theme } from './theme';

export interface Token {
  type: 'keyword' | 'string' | 'number' | 'boolean' | 'function' | 'type' | 'comment' | 'operator' | 'property' | 'command' | 'text';
  value: string;
}

export const colors: Record<Token['type'], string> = {
  keyword: theme.keyword,
  string: theme.string,
  number: theme.number,
  boolean: theme.boolean,
  function: theme.function,
  type: theme.type,
  comment: theme.comment,
  operator: theme.operator,
  property: theme.property,
  command: theme.command,
  text: theme.text,
};

export function tokenizeJitzu(code: string): Token[] {
  const tokens: Token[] = [];
  let remaining = code;
  let position = 0;

  const patterns = [
    { type: 'string' as const, regex: /^`[^`]*`/ },
    { type: 'string' as const, regex: /^"(?:[^"\\]|\\[\s\S])*"/ },
    { type: 'string' as const, regex: /^'(?:[^'\\]|\\[\s\S])*'/ },
    { type: 'number' as const, regex: /^0x[\da-f]+/i },
    { type: 'number' as const, regex: /^0b[01]+/ },
    { type: 'number' as const, regex: /^\d+\.?\d*(?:e[+-]?\d+)?[fl]?/i },
    { type: 'boolean' as const, regex: /^(true|false)\b/ },
    { type: 'keyword' as const, regex: /^(let|mut|const|fun|return|if|else|match|case|for|while|in|try|catch|finally|defer|async|await|yield|break|continue|enum|trait|impl|type|open|import|package|static|pub|public|private|internal|override|abstract|virtual|sealed|partial|where|when|is|as|new|delete|this|super|null|and|or|not|xor|mod|div|assert|like|union|label|unlabel|labels|alias)\b/ },
    { type: 'command' as const, regex: /^(cd|ls|cat|mkdir|rm|cp|mv|pwd|echo|grep|find|sort|head|tail|diff|touch|chmod|kill|killall|jobs|fg|bg|time|watch|tee|source|export|unalias|aliases)\b/ },
    { type: 'function' as const, regex: /^[a-zA-Z_]\w*(?=\s*\()/ },
    { type: 'type' as const, regex: /^[A-Z][a-zA-Z0-9_]*\b/ },
    { type: 'type' as const, regex: /^\.([A-Z]\w*)/ },
    { type: 'property' as const, regex: /^\.([a-z_]\w*)/ },
    { type: 'operator' as const, regex: /^(\+\+|--|==|!=|<=|>=|&&|\|\||->|=>|\.\.|\.\.\.|\?\?|[+\-*/%=!<>&|^~?:.])+/ },
    { type: 'text' as const, regex: /^[a-zA-Z_]\w*/ },
    { type: 'text' as const, regex: /^\s+/ },
    { type: 'text' as const, regex: /^[^\n]/ },
    { type: 'text' as const, regex: /^\n/ },
  ];

  while (remaining.length > 0) {
    let matched = false;

    // Handle comments first
    if (remaining.startsWith('//')) {
      const newlineIndex = remaining.indexOf('\n');
      const commentText = newlineIndex === -1 ? remaining : remaining.slice(0, newlineIndex);
      tokens.push({ type: 'comment', value: commentText });
      remaining = remaining.slice(commentText.length);
      position += commentText.length;
      matched = true;
    } else if (remaining.startsWith('/*')) {
      const endIndex = remaining.indexOf('*/');
      if (endIndex !== -1) {
        const commentText = remaining.slice(0, endIndex + 2);
        tokens.push({ type: 'comment', value: commentText });
        remaining = remaining.slice(commentText.length);
        position += commentText.length;
        matched = true;
      }
    }

    // Single quote strings
    if (!matched && remaining.startsWith("'")) {
      let endQuote = 1;
      let escaped = false;
      while (endQuote < remaining.length) {
        const char = remaining[endQuote];
        if (escaped) {
          escaped = false;
        } else if (char === '\\') {
          escaped = true;
        } else if (char === "'") {
          tokens.push({ type: 'string', value: remaining.slice(0, endQuote + 1) });
          remaining = remaining.slice(endQuote + 1);
          position += endQuote + 1;
          matched = true;
          break;
        }
        endQuote++;
      }
      if (!matched) {
        tokens.push({ type: 'text', value: remaining[0] });
        remaining = remaining.slice(1);
        position += 1;
        matched = true;
      }
    }

    // Template literals with interpolation
    if (!matched) {
      const templateMatch = remaining.match(/^`([^`]*)`/);
      if (templateMatch) {
        const content = templateMatch[1];
        if (content.includes('{') && content.includes('}')) {
          tokens.push({ type: 'string', value: '`' });
          let templateRemaining = content;
          while (templateRemaining.length > 0) {
            const braceStart = templateRemaining.indexOf('{');
            if (braceStart === -1) {
              tokens.push({ type: 'string', value: templateRemaining });
              break;
            }
            if (braceStart > 0) {
              tokens.push({ type: 'string', value: templateRemaining.slice(0, braceStart) });
            }
            const braceEnd = templateRemaining.indexOf('}', braceStart);
            if (braceEnd === -1) {
              tokens.push({ type: 'string', value: templateRemaining });
              break;
            }
            tokens.push({ type: 'operator', value: '{' });
            const interpolated = templateRemaining.slice(braceStart + 1, braceEnd);
            if (interpolated.match(/^[a-zA-Z_]\w*(\.[a-zA-Z_]\w*)*$/)) {
              const parts = interpolated.split('.');
              for (let i = 0; i < parts.length; i++) {
                if (i > 0) tokens.push({ type: 'operator', value: '.' });
                if (parts[i].match(/^[A-Z]/)) {
                  tokens.push({ type: 'type', value: parts[i] });
                } else {
                  tokens.push({ type: 'text', value: parts[i] });
                }
              }
            } else {
              tokenizeInterpolated(interpolated, tokens);
            }
            tokens.push({ type: 'operator', value: '}' });
            templateRemaining = templateRemaining.slice(braceEnd + 1);
          }
          tokens.push({ type: 'string', value: '`' });
        } else {
          tokens.push({ type: 'string', value: templateMatch[0] });
        }
        remaining = remaining.slice(templateMatch[0].length);
        position += templateMatch[0].length;
        matched = true;
      }
    }

    if (!matched) {
      for (const pattern of patterns) {
        if (pattern.regex.source.includes('`')) continue;
        const match = remaining.match(pattern.regex);
        if (match) {
          tokens.push({ type: pattern.type, value: match[0] });
          remaining = remaining.slice(match[0].length);
          position += match[0].length;
          matched = true;
          break;
        }
      }
    }

    if (!matched) {
      tokens.push({ type: 'text', value: remaining[0] });
      remaining = remaining.slice(1);
      position += 1;
    }
  }

  return tokens;
}

function tokenizeInterpolated(expr: string, tokens: Token[]): void {
  let remaining = expr;
  while (remaining) {
    const kwMatch = remaining.match(/^(let|mut|const|fun|return|if|else|match|case|for|while|in|try|catch|finally|defer|async|await|yield|break|continue|enum|trait|impl|type|open|import|package|static|pub|public|private|internal|override|abstract|virtual|sealed|partial|where|when|is|as|new|delete|this|super|null|and|or|not|xor|mod|div|assert|like|union|label|unlabel|labels|alias)\b/);
    if (kwMatch) {
      tokens.push({ type: 'keyword', value: kwMatch[0] });
      remaining = remaining.slice(kwMatch[0].length);
      continue;
    }
    const identMatch = remaining.match(/^[a-zA-Z_]\w*/);
    if (identMatch) {
      tokens.push({ type: identMatch[0].match(/^[A-Z]/) ? 'type' : 'text', value: identMatch[0] });
      remaining = remaining.slice(identMatch[0].length);
      continue;
    }
    const opMatch = remaining.match(/^(\+\+|--|==|!=|<=|>=|&&|\|\||->|=>|\.\.|\.\.\.|\?\?|[+\-*/%=!<>&|^~?:.])+/);
    if (opMatch) {
      tokens.push({ type: 'operator', value: opMatch[0] });
      remaining = remaining.slice(opMatch[0].length);
      continue;
    }
    const spaceMatch = remaining.match(/^\s+/);
    if (spaceMatch) {
      tokens.push({ type: 'text', value: spaceMatch[0] });
      remaining = remaining.slice(spaceMatch[0].length);
      continue;
    }
    tokens.push({ type: 'text', value: remaining[0] });
    remaining = remaining.slice(1);
  }
}

export function tokenToHtml(token: Token): string {
  const color = colors[token.type];
  const weight = token.type === 'keyword' ? ' font-weight:bold;' : '';
  const style = token.type === 'comment' ? ' font-style:italic;' : '';
  const escaped = token.value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;');
  return `<span style="color:${color};${weight}${style}">${escaped}</span>`;
}

export function highlightJitzu(code: string): string {
  return tokenizeJitzu(code).map(tokenToHtml).join('');
}
