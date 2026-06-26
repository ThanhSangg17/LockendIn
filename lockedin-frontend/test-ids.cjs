const https = require('https');
['3195394', '4761765', '4753995'].forEach(id => {
  https.get(`https://videos.pexels.com/video-files/${id}/${id}-hd_1920_1080_25fps.mp4`, res => {
    console.log(id, res.statusCode);
  });
});
