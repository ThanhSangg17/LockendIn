const https = require('https');

https.get('https://www.pexels.com/search/videos/gym/', {
  headers: {
    'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
  }
}, (res) => {
  let data = '';
  res.on('data', chunk => data += chunk);
  res.on('end', () => {
    const matches = data.match(/https:\/\/videos\.pexels\.com\/video-files\/[^"']+\.mp4/g);
    console.log(matches ? [...new Set(matches)].slice(0, 5) : 'no matches');
  });
}).on('error', (e) => {
  console.error(e);
});
