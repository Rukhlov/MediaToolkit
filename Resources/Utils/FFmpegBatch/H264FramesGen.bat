"..\..\FFmpeg4\bin\ffmpeg.exe" -f lavfi -i testsrc=duration=1:size=640x480:rate=30 -vcodec h264 -bf 0 -vframes 4 -pix_fmt yuv420p "output\testsrc_640x480_yuv420p_4IPframe.h264"
::"..\..\FFmpeg4\bin\ffmpeg.exe" -f lavfi -i testsrc=duration=1:size=640x480:rate=30 -vcodec h264 -vframes 1 -pix_fmt nv12 "output\testsrc_640x480_nv12_1frame.h264"
::"..\..\FFmpeg4\bin\ffmpeg.exe" -f lavfi -i testsrc=duration=1:size=640x480:rate=30 -vcodec h264 -vframes 4 -pix_fmt yuv420p "output\testsrc_640x480_yuv420p_4frame.h264"