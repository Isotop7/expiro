FROM golang:1.19
WORKDIR /app

COPY go.mod go.sum ./
RUN go mod download

COPY *.go ./
COPY . ./

RUN go build -v -o /app/isgood-backend

EXPOSE 5050

CMD ["/app/isgood-backend"]