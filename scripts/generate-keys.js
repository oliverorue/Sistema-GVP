const crypto = require('crypto');
const fs = require('fs');
const path = require('path');

const SCRIPTS_DIR = __dirname;

const { publicKey, privateKey } = crypto.generateKeyPairSync('rsa', {
  modulusLength: 2048,
  publicKeyEncoding: { type: 'spki', format: 'pem' },
  privateKeyEncoding: { type: 'pkcs8', format: 'pem' },
});

fs.writeFileSync(path.join(SCRIPTS_DIR, 'private.pem'), privateKey);
console.log('✓ private.pem saved (DO NOT COMMIT)');

console.log('\n=== PUBLIC KEY (copy this into license.ts) ===\n');
console.log(`const PUBLIC_KEY = \`${publicKey.trim()}\`;\n`);

console.log('=== LICENSE.TS CONSTANT (replace existing) ===\n');
const lines = publicKey.trim().split('\n');
const formatted = lines
  .map((l) => (l === '-----BEGIN PUBLIC KEY-----' || l === '-----END PUBLIC KEY-----' ? l : l))
  .join('\n');
console.log(`const PUBLIC_KEY = \`\n${formatted}\n\`;\n`);
