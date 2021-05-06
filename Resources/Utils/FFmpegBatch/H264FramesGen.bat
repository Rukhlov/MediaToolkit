"..\..\FFmpeg4\bin\ffmpeg.exe" -f lavfi -i testsrc=duration=1:size=640x480:rate=30 -vcodec h264 -vframes 4 -pix_fmt yuv420p "output\testsrc_640x480_yuv420p_4frame.h264"
