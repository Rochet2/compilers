FROM ubuntu:16.04

RUN apt update && apt install -y wget curl

# needed for compiling and running running
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
RUN apt update && apt install -y apt-transport-https
RUN echo "deb https://download.mono-project.com/repo/ubuntu stable-xenial main" | tee /etc/apt/sources.list.d/mono-official-stable.list
RUN apt update && apt install -y mono-complete

# setup working directory
WORKDIR /Interpreter
COPY . .
COPY ./Interpreter/TestCode.txt .

# build interpreter
RUN msbuild Interpreter.sln
RUN chmod +x Interpreter/bin/Debug/Interpreter.exe

CMD mono Interpreter/bin/Debug/Interpreter.exe TestCode.txt

# commands to build and run
# docker build -t compiler .
# docker run --rm -v $(pwd)/Interpreter/TestCode.txt:/Interpreter/TestCode.txt compiler

