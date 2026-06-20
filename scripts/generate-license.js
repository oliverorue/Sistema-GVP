const crypto = require('crypto');
const fs = require('fs');
const path = require('path');

const args = {};
process.argv.slice(2).forEach((arg) => {
  const match = arg.match(/^--(\w+)=(.+)$/);
  if (match) args[match[1]] = match[2];
});

if (!args.company || !args.machineId) {
  console.log('Usage: node generate-license.js --company="Ferretería X" --machineId="ABC123" [--expiresAt="2027-06-20"]');
  process.exit(1);
}

const privateKeyPath = path.join(__dirname, 'private.pem');
if (!fs.existsSync(privateKeyPath)) {
  console.error('ERROR: private.pem not found. Run generate-keys.js first.');
  process.exit(1);
}

const privateKey = fs.readFileSync(privateKeyPath, 'utf-8');

const license = {
  machineId: args.machineId,
  companyName: args.company,
  issuedAt: new Date().toISOString().split('T')[0],
};

if (args.expiresAt) {
  license.expiresAt = args.expiresAt;
}

const dataToSign = JSON.stringify({
  machineId: license.machineId,
  companyName: license.companyName,
  issuedAt: license.issuedAt,
  expiresAt: license.expiresAt,
});

const signer = crypto.createSign('SHA256');
signer.update(dataToSign);
const signature = signer.sign(privateKey, 'base64');

license.signature = signature;

const encoded = Buffer.from(JSON.stringify(license, null, 2)).toString('base64');

const safeName = args.company.replace(/[^a-zA-Z0-9_-]/g, '_');
const outputPath = path.join(__dirname, `license-${safeName}.txt`);
fs.writeFileSync(outputPath, encoded);

console.log(`✓ License generated: ${outputPath}`);
console.log(`\nLicense key (base64):\n${encoded}\n`);
