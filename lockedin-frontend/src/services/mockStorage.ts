// src/services/mockStorage.ts

/**
 * Simulates uploading a file by converting it to a Base64 string.
 * This acts as a Cloudinary or S3 local simulator.
 */
export function uploadFileMock(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onloadend = () => {
      if (typeof reader.result === 'string') {
        resolve(reader.result);
      } else {
        reject(new Error('Failed to read file as base64 string'));
      }
    };
    reader.onerror = () => {
      reject(reader.error || new Error('File read error'));
    };
    reader.readAsDataURL(file);
  });
}

/**
 * Generates a clean random mock URL for a specific asset type if file upload is skipped.
 */
export function getRandomMockImage(type: 'avatar' | 'cert' | 'evidence'): string {
  const images = {
    avatar: [
      'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&q=80&w=200',
      'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&q=80&w=200',
      'https://images.unsplash.com/photo-1599566150163-29194dcaad36?auto=format&fit=crop&q=80&w=200',
      'https://images.unsplash.com/photo-1580489944761-15a19d654956?auto=format&fit=crop&q=80&w=200'
    ],
    cert: [
      'https://images.unsplash.com/photo-1589330694653-ded6df53f7ec?auto=format&fit=crop&q=80&w=600',
      'https://images.unsplash.com/photo-1562774053-401386dfaa3d?auto=format&fit=crop&q=80&w=600'
    ],
    evidence: [
      'https://images.unsplash.com/photo-1557200134-90327ee9fafa?auto=format&fit=crop&q=80&w=600',
      'https://images.unsplash.com/photo-1586281380349-632531db7ed4?auto=format&fit=crop&q=80&w=600'
    ]
  };

  const list = images[type];
  return list[Math.floor(Math.random() * list.length)];
}
