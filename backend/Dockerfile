FROM golang:1.19
WORKDIR /app

ENV GIN_MODE=release

COPY go.mod go.sum ./
RUN go mod download
COPY *.go ./
COPY handlers ./
COPY models ./
COPY router ./

RUN CGO_ENABLED=0 GOOS=linux go build -o /app/isgood-backend

EXPOSE 5050

CMD ["/app/isgood-backend"]