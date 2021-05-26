"..\..\FFmpeg4\bin\ffmpeg.exe" -i "D:\Temp\test.mov" -ss 00:00:15.0 -t 5 -vcodec copy -bsf h264_mp4toannexb -an -f h264 "output\test_mov_annexb_1920x1080_5sec.h264"
::"..\..\FFmpeg4\bin\ffmpeg.exe" -i "D:\Temp\test.mov" -vcodec copy -bsf h264_mp4toannexb -an -f h264 "output\test_mov_annexb.h264"
::ffmpeg -i {some file} -vcodec copy -bsf h264_mp4toannexb -an -f {rawvideo|h264|whatever} out.h264