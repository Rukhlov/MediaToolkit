"..\..\FFmpeg4\bin\ffmpeg.exe" -f lavfi -i testsrc=duration=3600:size=320x240:rate=30  -pix_fmt yuv420p "output\testsrc.mp4"