FROM golang:1.21-alpine as builder
WORKDIR /app

COPY go.mod go.sum ./
RUN go mod download

COPY *.go ./
COPY . ./

RUN go build -v -o /app/isgood-backend

FROM alpine:3.19

COPY --from=builder /app/isgood-backend /app/isgood-backend

EXPOSE 5050

ENTRYPOINT [ "/app/isgood-backend" ]