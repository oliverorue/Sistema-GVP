const fs = require('fs');
const path = require('path');
const zlib = require('zlib');

const SIZE = 256;
const INDIGO = [79, 70, 229];
const BUILD_DIR = path.join(__dirname, '..', 'src', 'electron-app', 'build');

function createPNG(width, height, r, g, b) {
  const rawData = Buffer.alloc((width * height * 4) + height);
  for (let y = 0; y < height; y++) {
    rawData[y * (width * 4 + 1)] = 0;
    for (let x = 0; x < width; x++) {
      const offset = y * (width * 4 + 1) + 1 + x * 4;
      rawData[offset] = r;
      rawData[offset + 1] = g;
      rawData[offset + 2] = b;
      rawData[offset + 3] = 255;
    }
  }

  const deflated = zlib.deflateSync(rawData);

  function crc32(buf) {
    let crc = 0xFFFFFFFF;
    const table = new Int32Array(256);
    for (let i = 0; i < 256; i++) {
      let c = i;
      for (let j = 0; j < 8; j++) c = (c & 1) ? (0xEDB88320 ^ (c >>> 1)) : (c >>> 1);
      table[i] = c;
    }
    for (let i = 0; i < buf.length; i++) crc = table[(crc ^ buf[i]) & 0xFF] ^ (crc >>> 8);
    return (crc ^ 0xFFFFFFFF) >>> 0;
  }

  function chunk(type, data) {
    const len = Buffer.alloc(4);
    len.writeUInt32BE(data.length);
    const typeB = Buffer.from(type, 'ascii');
    const crcData = Buffer.concat([typeB, data]);
    const crcV = Buffer.alloc(4);
    crcV.writeUInt32BE(crc32(crcData));
    return Buffer.concat([len, typeB, data, crcV]);
  }

  const signature = Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]);
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(width, 0);
  ihdr.writeUInt32BE(height, 4);
  ihdr[8] = 8;
  ihdr[9] = 6;
  ihdr[10] = 0;
  ihdr[11] = 0;
  ihdr[12] = 0;

  return Buffer.concat([
    signature,
    chunk('IHDR', ihdr),
    chunk('IDAT', deflated),
    chunk('IEND', Buffer.alloc(0)),
  ]);
}

function createICO(pngBuffer) {
  const padding = pngBuffer.length % 4 ? 4 - (pngBuffer.length % 4) : 0;
  const totalSize = 6 + 16 + pngBuffer.length + padding;

  const ico = Buffer.alloc(totalSize);
  let offset = 0;

  ico.writeUInt16LE(0, offset); offset += 2;
  ico.writeUInt16LE(1, offset); offset += 2;
  ico.writeUInt16LE(1, offset); offset += 2;

  ico[offset] = 0; offset += 1;
  ico[offset] = 0; offset += 1;
  ico[offset] = 0; offset += 1;
  ico[offset] = 0; offset += 1;
  ico.writeUInt16LE(1, offset); offset += 2;
  ico.writeUInt16LE(32, offset); offset += 2;

  ico.writeUInt32LE(pngBuffer.length, offset); offset += 4;
  ico.writeUInt32LE(22, offset); offset += 4;

  pngBuffer.copy(ico, offset);

  return ico;
}

if (!fs.existsSync(BUILD_DIR)) fs.mkdirSync(BUILD_DIR, { recursive: true });

const png = createPNG(SIZE, SIZE, INDIGO[0], INDIGO[1], INDIGO[2]);
fs.writeFileSync(path.join(BUILD_DIR, 'icon.png'), png);
console.log('✓ Created build/icon.png');

const ico = createICO(png);
fs.writeFileSync(path.join(BUILD_DIR, 'icon.ico'), ico);
console.log('✓ Created build/icon.ico');

fs.writeFileSync(path.join(BUILD_DIR, 'icon.icns'), png);
console.log('✓ Created build/icon.icns (placeholder PNG, convert to .icns for macOS)');

console.log('\nIcons generated in:', BUILD_DIR);
