FROM golang:1.21-alpine as builder
WORKDIR /app

COPY go.mod go.sum ./
RUN go mod download

COPY *.go ./
COPY . ./

RUN go build -v -o /app/expiro-backend

FROM alpine:3.19

COPY --from=builder /app/expiro-backend /app/expiro-backend

EXPOSE 5050

ENTRYPOINT [ "/app/expiro-backend" ]